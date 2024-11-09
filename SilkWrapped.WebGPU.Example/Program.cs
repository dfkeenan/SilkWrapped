// See https://aka.ms/new-console-template for more information
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Dawn;
using SilkWrapped.WebGPU.Example;

//Environment.SetEnvironmentVariable("RUST_BACKTRACE", "full");

using var demo = new Demo();

demo.Run();

