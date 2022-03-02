using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TCSAssembler.Assembler
{
    public class X86 : Base
    {
        public X86()
        {
            ASM.Add("[org 0x7c00]");
            ASM.Add("[Bits 32]");
            ASM.Add("jmp Source.Kernel.Main");
            Instructions.Add(Code.Ldstr, LoadString);
        }

        public List<string> ASM = new();
        public delegate void Method(MethodDef Method, Instruction Instruction);
        private readonly Dictionary<Code, Method> Instructions = new();
        public int VIndex;

        public void ParseMethod(MethodDef Method)
        {
            if (Method.Name == ".cctor")  // Ignore useless class constructor
                return;

            ASM.Add($"\n{GetMethodName(Method)}:");
            if (Method.Body.Variables.Count > 0)
            {
                ASM.Add("\tpush rbp");
                ASM.Add("\tmov  rbp, rsp");
                ASM.Add("\t; /\\ We have variables!");
            }
            for (int CurrentInstruction = 0; CurrentInstruction < Method.Body.Instructions.Count; CurrentInstruction++)
            {
                Instruction Instruction = Method.Body.Instructions[CurrentInstruction];
                if (Instructions.ContainsKey(Instruction.OpCode.Code))
                    Instructions[Instruction.OpCode.Code].Invoke(Method, Instruction);
                ASM.Add($"\t; Instruction: {Instruction}");
            }
            VIndex = 0;
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
                    ASM.Add($"\t{GetFieldName(Type, variable)}: dq {(variable.HasConstant ? variable.Constant.Value + $" ;type: {variable.Constant.Type}" : 0)}");
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

        #region OPCODES/METHODS

        private void LoadString(MethodDef Method, Instruction Instruction)
        {
            if (Instruction.GetOperand() != null)
            {
                ASM.Add($"\n{GetMethodName(Method)}.{Method.Body.Variables[VIndex]}:");
                ASM.Add($"\t.size dd {Instruction.GetOperand().ToString().Length}");
                ASM.Add($"\t.data db \"{Instruction.GetOperand()}\", 0");
                ASM.Add($"\t.type db \"{Instruction.GetOperand().GetType()}\"");
                VIndex++;
            }
        }

        #endregion
    }
}