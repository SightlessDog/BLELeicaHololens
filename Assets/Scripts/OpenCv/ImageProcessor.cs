using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class ImageProcessor : Singleton<ImageProcessor>
{
    [SerializeField] private GameObject pointPrefab;
    private string filePath;
    private Matrix4x4 camToWorldMatrix;
    private PhotoCapture photoCaptureObject;
    private VideoCapture m_VideoCapture;
    private bool recording;
    private float frontFaceOffset = 0.0254f;
    private float pointThreshold = 0.2f;
    private float length;
    private bool aligned = true;
    private const int noMovementFrames = 10000;
    private float noMovementThreshold = 0.0001f;
    private Vector3[] previousLocations = new Vector3[noMovementFrames];
    private bool isMoving;

    void Awake()
    {
        //For good measure, set the previous locations
        for(int i = 0; i < previousLocations.Length; i++)
        {
            previousLocations[i] = Vector3.zero;
        }
    }

    private void Update()
    {
        // if (!aligned)
        // {
        //     PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        // }
        // if (BLEManager.Instance.getSubscribed())
        // {
        //     var hand = HandJointUtils.FindHand(Handedness.Right);
        //     if (hand.IsNotNull() && hand.TryGetJoint(TrackedHandJoint.IndexTip, out MixedRealityPose jointPose))
        //     {
        //         for(int i = 0; i < previousLocations.Length - 1; i++)
        //         {
        //             previousLocations[i] = previousLocations[i+1];
        //         }
        //         previousLocations[previousLocations.Length - 1] = jointPose.Position;
        //         for(int i = 0; i < previousLocations.Length - 1; i++)
        //         {
        //             if(Vector3.Distance(previousLocations[i], previousLocations[i + 100]) >= noMovementThreshold)
        //             {
        //                 //The minimum movement has been detected between frames
        //                 isMoving = true;
        //                 if (recording)
        //                 {
        //                     NotificationManager.Instance.SetNewNotification("Gonna stop recording");
        //                 }
        //                 break;
        //             }
        //             else
        //             {
        //                 isMoving = false;
        //                 NotificationManager.Instance.SetNewNotification("Gonna start recording");
        //                 // VideoCapture.CreateAsync(false, OnVideoCaptureCreated);
        //             }
        //         }
        //     }
        // }
    }
    
    public void TriggerShot(float value)
    {
        this.length = value;
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        Debug.Log("On Photo capture created");
        photoCaptureObject = captureObject;

        Resolution cameraResolution =
            PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Photo Mode started success");
            string filename = string.Format("test.jpg");
            filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            photoCaptureObject.TakePhotoAsync(OnCapturePhotoToMemory);
            photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturePhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame frame)
    {
        if (result.success)
        {
            Matrix4x4 cameraToWorldMatrix;
            frame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
            this.camToWorldMatrix = cameraToWorldMatrix;
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            float[] r = new float[9];
            float[] t = new float[9];
            sendReq(r, t);
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }

    public async Task sendReq(float[] r, float[] t)
    {
        await Task.Run(() =>
        {
            aligned = ImageProcessorApi.ProcessImage(filePath, r, t);
            if (aligned)
            {
                float3 firstPosition = new float3(t[0], t[1], t[2]);
                float3 secondPosition = new float3(t[3], t[4], t[5]);
                float3 thirdPosition = new float3(t[6], t[7], t[8]);
                
                float3 firstRot = new float3(r[0], r[1], r[2]);
                float3 secondRot = new float3(r[3], r[4], r[5]);
                float3 thirdRot = new float3(r[6], r[7], r[8]);
                
                firstPosition.y *= -1f;
                secondPosition.y *= -1f;
                thirdPosition.y *= -1f;
                firstPosition.x *= -1f;
                secondPosition.x *= -1f;
                thirdPosition.x *= -1f;

                Quaternion rotation1 =
                    CvUtils.RotationQuatFromRodrigues(firstRot);
                Quaternion rotation2 =
                    CvUtils.RotationQuatFromRodrigues(secondRot);
                Quaternion rotation3 =
                    CvUtils.RotationQuatFromRodrigues(thirdRot);
                
                Matrix4x4 transformUnityCamera = CvUtils.TransformInUnitySpace(firstPosition, rotation1);
                Matrix4x4 secondTransformUnityCamera = CvUtils.TransformInUnitySpace(secondPosition, rotation2);
                Matrix4x4 thirdTransformUnityCamera = CvUtils.TransformInUnitySpace(thirdPosition, rotation3);
                
                // Use camera to world transform to get world pose of marker
                Matrix4x4 transformUnityWorld = camToWorldMatrix * transformUnityCamera;
                Matrix4x4 transformUnityWorld2 = camToWorldMatrix * secondTransformUnityCamera;
                Matrix4x4 transformUnityWorld3 = camToWorldMatrix * thirdTransformUnityCamera;
                
                Vector3 startingPosition = CvUtils.GetVectorFromMatrix(transformUnityWorld);
                Vector3 startingPosition2 = CvUtils.GetVectorFromMatrix(transformUnityWorld2);
                Vector3 startingPosition3 = CvUtils.GetVectorFromMatrix(transformUnityWorld3);
                
                // NotificationManager.Instance.SetNewNotification(
                //     "Aruco Detected at " + CvUtils.GetVectorFromMatrix(transformUnityWorld), false);
                if (startingPosition.x < 0)
                {
                    startingPosition.x -= frontFaceOffset;
                    startingPosition2.x -= frontFaceOffset;
                    startingPosition3.x -= frontFaceOffset; 
                }
                else
                {
                    startingPosition.x += frontFaceOffset;
                    startingPosition2.x += frontFaceOffset;
                    startingPosition3.x += frontFaceOffset;
                }
                startingPosition.y += frontFaceOffset;
                startingPosition2.y += frontFaceOffset;
                startingPosition3.y += frontFaceOffset;
                startingPosition.z -= frontFaceOffset;
                startingPosition2.z = startingPosition.z;
                startingPosition3.z = startingPosition.z;
                TreatPoints(startingPosition, startingPosition2, startingPosition3,
                    CvUtils.GetQuatFromMatrix(transformUnityWorld));
            }
            else
            {
                NotificationManager.Instance.SetNewNotification("Some problem ", false);
            }
        });
    }

    private void TreatPoints(Vector3 startingPoint, Vector3 startingPoint2, Vector3 startingPoint3, Quaternion rotation)
    {
        // if (ShapeBuilder.Instance.getPolys().Count > 0)
        // {
        //     List<Vector3> existingPoints = ShapeBuilder.Instance.getPolys();
        //     Vector3 finishPoint = startingPoint + rotation * new Vector3(1, 0, 0) * length;
        //     // TODO this is also shame or am I wrong
        //     foreach (var p in existingPoints)
        //     {
        //         if ((startingPoint.x <= p.x + pointThreshold && startingPoint.x >= p.x - pointThreshold) &&
        //              (startingPoint.y <= p.y + pointThreshold && startingPoint.y >= p.y - pointThreshold) &&
        //              (startingPoint.z <= p.z + pointThreshold && startingPoint.z >= p.z - pointThreshold))
        //         {
        //             GameObject secPoi = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //             secPoi.transform.localScale = new Vector3(this.length, 0.01f, 0.01f);
        //             secPoi.transform.position = p;
        //             secPoi.transform.rotation = rotation;
        //             ShapeBuilder.Instance.addToPoly(finishPoint);
        //         }
        //         else if ((finishPoint.x <= p.x + pointThreshold && finishPoint.x >= p.x - pointThreshold) &&
        //                  (finishPoint.y <= p.y + pointThreshold && finishPoint.y >= p.y - pointThreshold) &&
        //                  (finishPoint.z <= p.z + pointThreshold && finishPoint.z >= p.z - pointThreshold))
        //         {
        //             GameObject secPoi = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //             Vector3 secondPoint = p + rotation * new Vector3(-1, 0, 0) * length;
        //             secPoi.transform.localScale = new Vector3(-this.length, 0.01f, 0.01f);
        //             secPoi.transform.position = p;
        //             secPoi.transform.rotation = rotation;
        //             ShapeBuilder.Instance.addToPoly(secondPoint);
        //         }
        //         else
        //         {
        //             GameObject firstPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //             firstPoint.transform.localScale = new Vector3(this.length, 0.01f, 0.01f);
        //             firstPoint.transform.position = startingPoint;
        //             firstPoint.transform.rotation = rotation;
        //             //GameObject secPoi = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //             //secPoi.transform.position = secondPoint;
        //             ShapeBuilder.Instance.addToPoly(startingPoint);
        //             ShapeBuilder.Instance.addToPoly(finishPoint);
        //         }
        //     }
        // }
        // else\
        // {
        //     GameObject firstPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //     firstPoint.transform.localScale = new Vector3(this.length, 0.01f, 0.01f);
        //     firstPoint.transform.position = startingPoint;
        //     firstPoint.transform.rotation = rotation;
        //     //GameObject secPoi = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //     Vector3 secondPoint = startingPoint + rotation * new Vector3(1, 0, 0) * length;
        //     //secPoi.transform.position = secondPoint;
        //     ShapeBuilder.Instance.addToPoly(startingPoint);
        //     ShapeBuilder.Instance.addToPoly(secondPoint);
        // }
        GameObject firstPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        firstPoint.transform.position = startingPoint;
        GameObject secondPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        secondPoint.transform.position = startingPoint2;
        GameObject thirdPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        thirdPoint.transform.position = startingPoint3;
    }
}