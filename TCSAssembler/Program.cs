using dnlib.DotNet;
using System.Diagnostics;
using TCSAssembler.Assembler;

namespace TCSAssembler
{
    public static class Program
    {
        public static X86 Assembler { get; } = new();
        public const string Input = "..\\..\\..\\..\\Kernel\\bin\\" + Mode + "\\net6.0\\Kernel.dll";
        public const string Output = "..\\..\\..\\..\\Kernel\\bin\\" + Mode + "\\net6.0\\Kernel.bin";
        public const string OutputASM = "..\\..\\..\\..\\Kernel\\bin\\" + Mode + "\\net6.0\\Kernel.asm";
        public const string Nasm = "..\\..\\..\\nasm.exe";
        public const string Mode = "Debug";

        public static void Main()
        {
            Console.WriteLine("Loading dll...");
            ModuleDefMD Code = ModuleDefMD.Load(Input);
            Console.WriteLine("Compiling...");
            foreach (var Class in Code.Types)
            {
                foreach (var Method in Class.Methods)
                {
                    Assembler.ParseMethod(Method);
                }
                Assembler.ParseFields(Class); //and parse fields
            }
            Console.WriteLine("Finalizing...");
            Assembler.Export(OutputASM);
            Console.WriteLine("Running NASM...");
            Process.Start(Nasm, OutputASM + " -o " + Output + " -O3");
            Console.WriteLine("Done!");
        }
    }
}