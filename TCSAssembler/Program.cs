using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using TCSAssembler.Assembler.X86;

/* IMPORTANT!
    -> TypeDef can be a class!
    -> FieldDef could be a variable
*/

namespace TCSAssembler
{
    public static class Program
    {
        
        public static void Main(string[] args)
        {
            ModuleDefMD _code=ModuleDefMD.Load("..\\Kernel\\bin\\Debug\\net6.0\\Kernel.dll");
            // /\ this is to get the IL code from the .dll file
            //later on we have to change this to args[0]
            //so we can use TCSAssembler kernel.dll
            //or no :p
            CoreX86.Initialise(); //initialise the code
            foreach (var _class in _code.Types)
            {
                foreach (var method in _class.Methods)
                {
                    CoreX86.ParseMethod(method);
                }
                CoreX86.ParseFields(_class); //and parse fields
            }
            CoreX86.Export("Kernel.asm");
            Console.WriteLine("Done!");
        }
    }
}