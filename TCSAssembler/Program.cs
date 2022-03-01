namespace TCSAssembler
{
    public static class Program
    {
        public static void Main()
        {
            Assembler.X86.CoreX86.Init();
            Kernel.Kernel.KernelEntry();
            Assembler.X86.CoreX86.Export("Kernel.asm");
        }
    }
}