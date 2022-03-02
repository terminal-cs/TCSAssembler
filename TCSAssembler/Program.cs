using dnlib.DotNet;
using System.Diagnostics;
using TCSAssembler.Assembler.X86;

/* IMPORTANT!
    -> TypeDef can be a class!
    -> FieldDef could be a variable
*/

namespace TCSAssembler
{
    public static class Program
    {
        public const string Input = "..\\..\\..\\..\\Kernel\\bin\\Debug\\net6.0\\Kernel.dll";
        public const string Output = "..\\..\\..\\..\\Kernel\\bin\\Debug\\net6.0\\Kernel.bin";
        public const string OutputASM = "..\\..\\..\\..\\Kernel\\bin\\Debug\\net6.0\\Kernel.asm";
        public const string Nasm = "..\\..\\..\\nasm.exe";
        public static void Main()
        {
            CoreX86.Initialise();
            Console.WriteLine("Loading dll...");
            ModuleDefMD Code = ModuleDefMD.Load(Input);
            Console.WriteLine("Compiling...");
            foreach (var Class in Code.Types)
            {
                foreach (var Method in Class.Methods)
                {
                    CoreX86.ParseMethod(Method);
                }
                CoreX86.ParseFields(Class); //and parse fields
            }
            Console.WriteLine("Finalizing...");
            CoreX86.Export(OutputASM);
            Console.WriteLine("Running NASM...");
            Process.Start(Nasm, OutputASM + " -o " + Output + " -O3");
            Console.WriteLine("Done!");
        }
    }
}