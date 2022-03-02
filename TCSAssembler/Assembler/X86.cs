using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TCSAssembler.Assembler
{
    public class X86 : Base
    {
        public X86()
        {
            ASM.Add("[org 0x7c00]");
            ASM.Add("jmp Source.Kernel.Main\n");
        }

        public readonly List<string> ASM = new();
        public int VIndex;

        public void ParseMethod(MethodDef Method)
        {
            if (Method.Name == ".cctor")  // Ignore useless class constructor
                return;

            ASM.Add($"{GetMethodName(Method)}:");
            for (int CurrentInstruction = 0; CurrentInstruction < Method.Body.Instructions.Count; CurrentInstruction++)
            {
                Instruction Instruction = Method.Body.Instructions[CurrentInstruction];
                if (Instruction.ToString().EndsWith(" nop") || Instruction.ToString().EndsWith(" ret"))
                {
                    continue;
                }
                if (Instruction.ToString().Contains("ldstr ") && Method.Body.Instructions[CurrentInstruction + 1].ToString().Contains("call System.Void System.Console::WriteLine(System.String)"))
                {
                    ASM.Add("  " + Method.Body.Variables[VIndex] + " db " + Instruction.ToString().Split("ldstr ")[1] + ", 0");
                    ASM.Add("  mov [" + Method.Body.Variables[VIndex++] + "], ah");
                    CurrentInstruction++;
                    continue;
                }
                ASM.Add($"  ; Instruction: {Instruction}");
            }
            /*for (int i=0;i<method.Body.Variables.Count;i++) {
                var type=method.Body.Variables[i].Type;
                Console.WriteLine(type.ToString());
                code.Add($"{method.DeclaringType.Namespace}.{method.Name}.{method.Body.Variables[i].Name}:");
                code.Add($"\t.size dd {method.Body.Variables[i]}");
                //code.Add($"; {method.Body.Variables[i].Name} type: {method.Body.Variables[i].Type}");
                code.Add($"; {method.Body.Variables[i].Name} type: {method.Body.Variables[i].Type}");
            }*/
        }

        public void ParseFields(TypeDef Type)
        {
            foreach (var variable in Type.Fields)
            {
                if (variable.IsStatic)
                {
                    ASM.Add($"  {GetFieldName(Type, variable)}: dq {(variable.HasConstant ? variable.Constant.Value + $" ;type: {variable.Constant.Type}" : 0)}");
                    //format the name, to make it readable
                    /*
                    DB	Define Byte	allocates 1 byte
                    DW	Define Word	allocates 2 bytes
                    DD	Define Doubleword	allocates 4 bytes
                    DQ	Define Quadword	allocates 8 bytes
                    DT	Define Ten Bytes	allocates 10 bytes

                    EXAMPLE:
                    choice		DB	'y'
                    number		DW	12345
                    neg_number	DW	-12345
                    big_number	DQ	123456789
                    real_number1	DD	1.234
                    real_number2	DQ	123.456
                    */
                }
            }
        }

        public void Export(string Path)
        {
            ASM.Add("\ntimes 510-($-$$) db 0");
            ASM.Add("dw 0xAA55");
            string Final = "";
            int k = 0;
            foreach (string line in ASM)
            {
                Final += (k == ASM.Count - 1 ? line : line + '\n');
                k++;
            }
            File.WriteAllText(Path, Final);
        }
    }
}