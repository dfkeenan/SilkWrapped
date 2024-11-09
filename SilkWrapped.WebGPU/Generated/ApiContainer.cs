namespace SilkWrapped.WebGPU
{
    public partial class ApiContainer : System.IDisposable
    {
        public void Dispose()
        {
            Core.Dispose();
        }
    }
}