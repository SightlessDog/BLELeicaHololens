using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class NewBehaviourScript : MonoBehaviour
{
#if UNITY_EDITOR
    private string filePath = "C://Users/ElyessEleuch/Desktop/projects/HOBLE/Assets/Image/test.jpg";
    private string modelPath;
    private string patternPath;
    private string classListPath;
    private string testImagePath;
#else
    private string filePath;
    private string modelPath;
    private string patternPath;
    private string classListPath;
#endif
    void Start()
    {
        modelPath = Path.Combine(Application.streamingAssetsPath, "best.onnx");
        patternPath = Path.Combine(Application.streamingAssetsPath, "pattern.jpg");
        classListPath = Path.Combine(Application.streamingAssetsPath, "classes.txt");
#if UNITY_EDITOR
        testImagePath = Path.Combine(Application.streamingAssetsPath, "test.jpg");
        Debug.Log("gonna send req");
        ImageProcessor.Output center = ImageProcessor.ProcessImage(testImagePath, modelPath, patternPath, classListPath);
        Debug.Log("Bla bla car  " + center.x + " " + center.y + " " + center.z);
#else
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
#endif
    }

    private PhotoCapture photoCaptureObject = null;

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
            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("gonna send request");
            ImageProcessor.Output center = ImageProcessor.ProcessImage(filePath, modelPath, patternPath, classListPath);
            Debug.Log("Bla bla car " + center.x + " " + center.y);
            //NotificationManager.Instance.SetNewNotification("center was detected " + center.tvec + " " + center.angles);
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        Debug.Log("[DEBUG] EE on capture to memory");
        if (result.success)
        {
            Debug.Log("[DEBUG] EE success");
            Resolution cameraResolution = PhotoCapture.SupportedResolutions
                .OrderByDescending((res) => res.width * res.height).First();
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            // In this example, we captured the image using the BGRA32 format.
            // So our stride will be 4 since we have a byte for each rgba channel.
            // The raw image data will also be flipped so we access our pixel data
            // in the reverse order.
            int stride = 4;
            float denominator = 1.0f / 255.0f;

            List<Color> colorArray = new List<Color>();

            for (int i = imageBufferList.Count - 1; i >= 0; i -= stride)
            {
                float a = (int)(imageBufferList[i - 0]) * denominator;
                float r = (int)(imageBufferList[i - 1]) * denominator;
                float g = (int)(imageBufferList[i - 2]) * denominator;
                float b = (int)(imageBufferList[i - 3]) * denominator;

                colorArray.Add(new Color(r, g, b, a));
            }

            for (int i = 0; i < colorArray.Count; i++)
            {
                Debug.Log(colorArray[i].ToString());
            }
            // Now we could do something with the array such as texture.SetPixels() or run image processing on the list
        }

        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }
}