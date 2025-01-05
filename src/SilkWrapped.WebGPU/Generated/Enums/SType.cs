namespace SilkWrapped.WebGPU;
public enum SType
{
    SurfaceDescriptorFromCanvasHtmlSelector = 4,
    ShaderModuleSpirvDescriptor = 5,
    ShaderModuleWgslDescriptor = 6,
    Invalid = 0,
    SurfaceDescriptorFromMetalLayer = 1,
    SurfaceDescriptorFromWindowsHwnd = 2,
    SurfaceDescriptorFromXlibWindow = 3,
    [Obsolete("Deprecated in favour of \"SurfaceDescriptorFromCanvasHtmlSelector\"")]
    SurfaceDescriptorFromCanvasHtmlselector = 4,
    [Obsolete("Deprecated in favour of \"ShaderModuleSpirvDescriptor\"")]
    ShaderModuleSpirvdescriptor = 5,
    [Obsolete("Deprecated in favour of \"ShaderModuleWgslDescriptor\"")]
    ShaderModuleWgsldescriptor = 6,
    PrimitiveDepthClipControl = 7,
    SurfaceDescriptorFromWaylandSurface = 8,
    SurfaceDescriptorFromAndroidNativeWindow = 9,
    SurfaceDescriptorFromXcbWindow = 10,
    RenderPassDescriptorMaxDrawCount = 15,
    Force32 = int.MaxValue
}