using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkWrapped.WebGPU;
public static class BlendStates
{
    public static BlendState Default => new BlendState()
    {
        Color = new BlendComponent
        {
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        },
        Alpha = new BlendComponent
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        }
    };

    public static BlendState Additive => new BlendState
    {
        Color = new BlendComponent
        {
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.One,
            Operation = BlendOperation.Add
        },
        Alpha = new BlendComponent
        {
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.One,
            Operation = BlendOperation.Add
        }
    };

    public static BlendState AlphaBlend => new BlendState
    {
        Color = new BlendComponent
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        },
        Alpha = new BlendComponent
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        }
    };

    public static BlendState NonPremultiplied => new BlendState
    {
        Color = new BlendComponent
        {
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        },
        Alpha = new BlendComponent
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        }
    };
}
