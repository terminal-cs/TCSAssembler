using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TCSAssembler.Assembler
{
    public class X86 : Base
    {
        public X86()
        {
            Instructions.Add(Code.Ldstr, LoadString); //LOADSTRING (e.g "string a="hello" " )
            Instructions.Add(Code.Ret, Ret);        //RET        (e.g "return;"           )
            Instructions.Add(Code.Call, Call);       //CALLS      (e.g "hello_world()"     )
            Instructions.Add(Code.Ldc_I4, LdcI4);      //INTEGER    (e.g "int a=5"           )
        }

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

        public Dictionary<string, List<string>> ASM = new();
        public delegate void Method(MethodDef Method, Instruction Instruction, int Index);
        private readonly Dictionary<Code, Method> Instructions = new();
        public int VIndex;
        public int SIndex;

        public void ParseMethod(MethodDef Method)
        {
            if (Method.Name == ".cctor")  // Ignore useless class constructor
                return;

            string MethodName = GetMethodName(Method);

            ASM.Add($"{MethodName}", new());
            for (int CI = 0; CI < Method.Body.Instructions.Count; CI++)
            {
                Instruction Instruction = Method.Body.Instructions[CI];
                switch (Instruction.OpCode.Code)
                {
                    case Code.Nop:
                        continue;
                    default:
                        if (Instructions.ContainsKey(Instruction.OpCode.Code))
                            Instructions[Instruction.OpCode.Code].Invoke(Method, Instruction, CI);
                        //ASM[MethodName].Add($"\t; Instruction: {Instruction}");
                        break;
                }
            }
            VIndex = 0;
        }

        public void ParseFields(TypeDef Type)
        {
            foreach (var variable in Type.Fields)
            {
                if (variable.IsStatic)
                {
                    ASM.Add(GetFieldName(Type, variable), new());
                    ASM[GetFieldName(Type, variable)].Add($"\tdq {(variable.HasConstant ? variable.Constant.Value + $" ;type: {variable.Constant.Type}" : 0)}");
                    //format the name, to make it readable
                }
                else
                {
                    Console.WriteLine("Non-static items are not supported yet!");
                }
            }
        }

        public void Export(string Path)
        {
            string Final = "[org 0x7c00]\n\n%include \"..\\..\\..\\Libraries\\System\\Console.asm\"\njmp Source.Kernel.Main\n";

            foreach (KeyValuePair<string, List<string>> Pair in ASM)
            {
                if (Pair.Key?.Length == 0)
                {
                    foreach (string poc in Pair.Value)
                    {
                        Final += $"{poc}";
                    }
                }
                else
                {
                    Final += $"\n{Pair.Key}:\n";
                    foreach (string poc in Pair.Value)
                    {
                        Final += $"{poc}\n";
                    }
                }
                // do something with entry.Value or entry.Key
            }
            Final += "\ntimes 510-($-$$) db 0\ndw 0xAA55";
            File.WriteAllText(Path, Final);
        }

        #region OPCODES/METHODS

        private void LoadString(MethodDef Method, Instruction Instruction, int Index)
        {
            Code[] codes = { Code.Stloc, Code.Stloc_0, Code.Stloc_1, Code.Stloc_2, Code.Stloc_3, Code.Stloc_S };
            if (codes.Contains(Method.Body.Instructions[Index + 1].GetOpCode().Code))
            {
                //it's a var
                string vname = GetMethodName(Method) + "." + Method.Body.Variables[VIndex];
                ASM.Add(vname, new());
                ASM[vname].Add($"\t.size dd {Instruction.GetOperand().ToString().Length}");
                ASM[vname].Add($"\t.data db \"{Instruction.GetOperand()}\", 13, 10, 0");
                ASM[vname].Add($"\t.type db \"{Instruction.GetOperand().GetType()}\"");
                VIndex++;
            }
            else
            {
                string vname = GetMethodName(Method) + ".S_" + SIndex;
                if (Method.Body.Instructions[Index + 1].OpCode.Code == Code.Call)
                {
                    ASM[GetMethodName(Method)].Add($"\tmov si, {vname}.data");
                }
                ASM.Add(vname, new());
                ASM[vname].Add($"\t.size dd {Instruction.Operand.ToString().Length}");
                ASM[vname].Add($"\t.data db \"{Instruction.Operand}\", 13, 10, 0");
                ASM[vname].Add($"\t.type db \"{Instruction.Operand.GetType()}\"");
                SIndex++;
            }
        }

        private void Ret(MethodDef Method, Instruction Instruction, int Index)
        {
            ASM[GetMethodName(Method)].Add("\tret");
        }

        private void Call(MethodDef Method, Instruction Instruction, int Index)
        {
            string name = Instruction.Operand.ToString();
            string type = name.Split()[0];
            string func = name.Split()[1];
            func = func.Replace("::", ".");
            func = func.Remove(func.IndexOf('('));
            ASM[GetMethodName(Method)].Add($"\tcall {func}");
        }

        private void LdcI4(MethodDef Method, Instruction Instruction, int Index)
        {
        }

        #endregion
    }
}