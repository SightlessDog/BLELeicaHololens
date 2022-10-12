using System.Runtime.InteropServices;

public class ImageProcessor
{
#if UNITY_EDITOR
    const string Opencvunitydll = "OpenCvUnityDllx64.dll";
#else
    const string Opencvunitydll = "OpenCvUnityDll.dll";
#endif

    public struct Output
    {
        public float x;
        public float y;
        public float z;
    }

    [DllImport(Opencvunitydll, EntryPoint = "ProcessImage", CharSet = CharSet.Unicode)]
    public static extern Output ProcessImage(string filePath, string modelPath, string patternPath, string classListPath);
}