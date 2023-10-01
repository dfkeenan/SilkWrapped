using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkWrapped.WebGPU;
public unsafe partial class TextureWrapper
{
    public TextureViewWrapper CreateView()
    {
        var result = Api.Core.TextureCreateView(Handle, null);
        if (result == null)
            return null;
        return new TextureViewWrapper(Api, result);
    }
}
