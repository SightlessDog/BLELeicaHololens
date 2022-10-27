using System.Runtime.InteropServices;

public class ImageProcessorApi
{
#if UNITY_EDITOR
    const string Opencvunitydll = "OpenCvUnityDllx64.dll";
#else
    const string Opencvunitydll = "OpenCvUnityDll.dll";
#endif

    [DllImport(Opencvunitydll, EntryPoint = "ProcessImage", CharSet = CharSet.Unicode)]
    public static extern void ProcessImage(string filePath, float[] r, float[] t);
}