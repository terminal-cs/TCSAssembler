using Console = TCSAssembler.Assembler.X86.Console;

namespace TCSAssembler.Kernel
{
    public static class Kernel
    {
        public static void KernelEntry()
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("This was compiled from dotnet 6.0 c#!");
        }
    }
}
