using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class ImageProcessor : Singleton<ImageProcessor>
{
#if UNITY_EDITOR
    private string testImagePath;
#endif
    [SerializeField] private GameObject point;
    private string filePath;
    private Matrix4x4 camToWorldMatrix;
    private PhotoCapture photoCaptureObject;
    private float front_face_offset = 0.0254f;
    private float length;

    void Start()
    {
#if UNITY_EDITOR
        testImagePath = Path.Combine(Application.streamingAssetsPath, "test8.jpg");
        camToWorldMatrix = Matrix4x4.identity;
        Debug.Log("gonna send req");
        float[] r = new float[3];
        float[] t = new float[3];
        ImageProcessorApi.ProcessImage(testImagePath, r, t);
        float3 position = new float3(0.1612205f, -0.05495432f, 0.4074616f);
        float3 rot = new float3(r[0], r[1], r[2]);
        Quaternion rotation =
            CvUtils.RotationQuatFromRodrigues(rot);
        Matrix4x4 transformUnityCamera =
            CvUtils.TransformInUnitySpace(position, new Quaternion(0.71567f, 0.67648f, 0.10675f, -0.13709f));
        // Use camera to world transform to get world pose of marker
        Matrix4x4 transformUnityWorld = camToWorldMatrix * transformUnityCamera;
        Instantiate(point, CvUtils.GetVectorFromMatrix(transformUnityWorld),
            CvUtils.GetQuatFromMatrix(transformUnityWorld));
#endif
    }

    public void TriggerShot(float value)
    {
        this.length = value;
        NotificationManager.Instance.SetNewNotification("Shot triggered");
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

    public async Task sendReq(float[] r, float[] t)
    {
        await Task.Run(() =>
        {
            ImageProcessorApi.ProcessImage(filePath, r, t);
            if (t[0] == 0 && t[1] == 0 && t[2] == 0)
            {
                NotificationManager.Instance.SetNewNotification("Aruco not detected ", false);
            }
            else
            {
                float3 position = new float3(t[0], t[1], t[2]);
                float3 rot = new float3(r[0], r[1], r[2]);
                position.y *= -1f;
                position.x *= -1f;
                Quaternion rotation =
                    CvUtils.RotationQuatFromRodrigues(rot);
                Matrix4x4 transformUnityCamera = CvUtils.TransformInUnitySpace(position, rotation);
                // Use camera to world transform to get world pose of marker
                Matrix4x4 transformUnityWorld = camToWorldMatrix * transformUnityCamera;
                Vector3 poiPosition = CvUtils.GetVectorFromMatrix(transformUnityWorld);
                NotificationManager.Instance.SetNewNotification(
                    "Aruco Detected at " + CvUtils.GetVectorFromMatrix(transformUnityWorld), false);
                GameObject poi = Instantiate(point, Vector3.zero, Quaternion.identity) as GameObject;
                poi.transform.localScale = new Vector3(this.length, 0, 0);
                if (poiPosition.x < 0)
                {
                    poiPosition.x -= front_face_offset;
                }
                else
                {
                    poiPosition.x += front_face_offset;
                }
                poiPosition.y += front_face_offset;
                poiPosition.z -= front_face_offset;
                poi.transform.position = poiPosition;
                poi.transform.rotation = CvUtils.GetQuatFromMatrix(transformUnityWorld);
            }
        });
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Cam matrix " + camToWorldMatrix);
            float[] r = new float[3];
            float[] t = new float[3];
            sendReq(r, t);
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }
}