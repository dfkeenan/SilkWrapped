namespace SilkWrapped.WebGPU;
public abstract class CustomRenderPipeline : IEquatable<CustomRenderPipeline>, IDisposable
{
    private bool disposedValue;

    public RenderPipeline RenderPipeline { get; protected set; }

    public bool Equals(CustomRenderPipeline? other)
    {
        if (other is not CustomRenderPipeline) return false;

        return RenderPipeline.Equals(other.RenderPipeline);
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj as CustomRenderPipeline);
    }

    public override int GetHashCode()
    {
        return RenderPipeline.Handle.GetHashCode();
    }

    public static implicit operator RenderPipelineHandle(CustomRenderPipeline pipeline) => pipeline.RenderPipeline;

    protected virtual void Disposing() { }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                RenderPipeline?.Dispose();
                Disposing();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
