using System.Runtime.InteropServices;

public class ImageProcessor
{
#if UNITY_EDITOR
    const string Opencvunitydll = "OpenCvUnityDllx64.dll";
#else
    const string Opencvunitydll = "OpenCvUnityDll.dll";
#endif
    
    public struct Center
    {
        public float x;
        public float y;
    };
      
    
    [DllImport(Opencvunitydll, EntryPoint = "ProcessFrame")]
    private static extern byte[] ProcessFrame();
    
    [DllImport(Opencvunitydll, EntryPoint = "ProcessImage", CharSet = CharSet.Unicode)]
    public static extern Center ProcessImage(string filePath);
}