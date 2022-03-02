using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TCSAssembler.Assembler
{
    public class X86 : Base
    {
        public X86()
        {
            Instructions.Add(Code.Ldstr, LoadString);
            Instructions.Add(Code.Ret, Ret);
            Instructions.Add(Code.Call, Call);
        }

        public Dictionary<string, List<string>> ASM = new();
        public delegate void Method(MethodDef Method, Instruction Instruction, int Index);
        private Dictionary<Code, Method> Instructions=new();
        public int VIndex=0;
        public int SIndex=0;

        public void ParseMethod(MethodDef Method)
        {
            if (Method.Name == ".cctor")  // Ignore useless class constructor
                return;
            
            string MethodName=GetMethodName(Method);

            ASM.Add($"{MethodName}", new());
            if (Method.Body.Variables.Count>0) {
                ASM[MethodName].Add("\tpush bpl");
                ASM[MethodName].Add("\tmov  bpl, spl");
                ASM[MethodName].Add("\t; /\\ We have variables!");
            }
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
                        ASM[MethodName].Add($"\t; Instruction: {Instruction}");
                        break;
                }
                /*if (Instruction.OpCode.Code!=Code.Nop) {
                    if (Instructions.ContainsKey(Instruction.OpCode.Code))
                        Instructions[Instruction.OpCode.Code].Invoke(Method, Instruction, CI);
                    ASM[MethodName].Add($"\t; Instruction: {Instruction}");
                }*/
            }
            VIndex=0;
        }

        public void ParseFields(TypeDef Type)
        {
            foreach (var variable in Type.Fields)
            {
                if (variable.IsStatic)
                {
                    //ASM.Add($"\t{GetFieldName(Type, variable)}: dq {(variable.HasConstant ? variable.Constant.Value + $" ;type: {variable.Constant.Type}" : 0)}");
                    ASM.Add(GetFieldName(Type, variable), new());
                    ASM[GetFieldName(Type, variable)].Add($"\tdq {(variable.HasConstant ? variable.Constant.Value + $" ;type: {variable.Constant.Type}" : 0)}");
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
            //ASM["_."].Add("\ntimes 510-($-$$) db 0");
            //ASM["_."].Add("dw 0xAA55");
            string Final = "[ORG 0x7c00]\n";
            //Final+="[BITS 32]\n";
            Final+="jmp Source.Kernel.Main\n\n%include \"Libraries/System/Console.asm\"\n\n";

            foreach(KeyValuePair<string, List<string>> entry in ASM)
            {
                if (entry.Key=="") {
                    foreach(string poc in entry.Value) {
                        Final+=$"{poc}\n";
                    }
                } else {

                    Final+=$"{entry.Key}:\n";
                    foreach(string poc in entry.Value) {
                        Final+=$"{poc}\n";
                    }
                }
                // do something with entry.Value or entry.Key
            }
            /*foreach (string line in ASM)
            {
                Final += (k == ASM.Count - 1 ? line : line + '\n');
                k++;
            }*/
            Final+="\ntimes 510-($-$$) db 0\n";
            Final+="dw 0xAA55\n";
            File.WriteAllText(Path, Final);
        }

        #region OPCODES/METHODS

        private void LoadString(MethodDef Method, Instruction Instruction, int Index) {
            Code[] codes={ Code.Stloc, Code.Stloc_0, Code.Stloc_1, Code.Stloc_2, Code.Stloc_3, Code.Stloc_S };
            if (codes.Contains(Method.Body.Instructions[Index+1].GetOpCode().Code)) {
                //it's a var
                string vname=GetMethodName(Method) + "." + Method.Body.Variables[VIndex];
                ASM.Add(vname, new());
                ASM[vname].Add($"\t.size dd {Instruction.GetOperand().ToString().Length}");
                ASM[vname].Add($"\t.data db \"{Instruction.GetOperand()}\", 13, 10, 0");
                ASM[vname].Add($"\t.type db \"{Instruction.GetOperand().GetType()}\"");
                VIndex++;
            } else {
                string vname=GetMethodName(Method) + ".S_" + SIndex;
                if (Method.Body.Instructions[Index+1].OpCode.Code==Code.Call) {
                    ASM[GetMethodName(Method)].Add($"\tmov si, {vname}.data");
                }
                ASM.Add(vname, new());
                ASM[vname].Add($"\t.size dd {Instruction.Operand.ToString().Length}");
                ASM[vname].Add($"\t.data db \"{Instruction.Operand}\", 13, 10, 0");
                ASM[vname].Add($"\t.type db \"{Instruction.Operand.GetType()}\"");
                SIndex++;
            }
        }

        private void Ret(MethodDef Method, Instruction Instruction, int Index) {
            ASM[GetMethodName(Method)].Add("\tret");
        }

        private void Call(MethodDef Method, Instruction Instruction, int Index) {
            string name=Instruction.Operand.ToString();
            string type=name.Split()[0];
            string func=name.Split()[1];
            func=func.Replace("::", ".");
            func=func.Remove(func.IndexOf('('));
            ASM[GetMethodName(Method)].Add($"\tcall {func}");
        }

        #endregion
    }
}