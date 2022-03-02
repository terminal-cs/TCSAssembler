using dnlib.DotNet;
using TCSAssembler.Assembler.X86;

/* IMPORTANT!
    -> TypeDef can be a class!
    -> FieldDef could be a variable
*/

namespace TCSAssembler
{
    public static class Program
    {
        public static void Main()
        {
            CoreX86.Initialise();
            ModuleDefMD Code = ModuleDefMD.Load("..\\..\\..\\..\\Kernel\\bin\\Debug\\net6.0\\Kernel.dll");
            foreach (var Class in Code.Types)
            {
                foreach (var Method in Class.Methods)
                {
                    CoreX86.ParseMethod(Method);
                }
                CoreX86.ParseFields(Class); //and parse fields
            }
            CoreX86.Export("Kernel.asm");
            Console.WriteLine("Done!");
        }
    }
}