namespace SilkWrapped.WebGPU;
public enum PresentMode
{
    Fifo = 0,
    FifoRelaxed = 1,
    Immediate = 2,
    Mailbox = 3,
    Force32 = int.MaxValue
}