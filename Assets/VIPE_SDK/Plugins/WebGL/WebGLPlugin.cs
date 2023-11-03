using System.Runtime.InteropServices;

public static class WebGLPlugin
{
    [DllImport("__Internal")]
    public static extern void CallJavaScriptFunction();
}