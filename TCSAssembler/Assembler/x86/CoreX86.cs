using dnlib.DotNet;

namespace TCSAssembler.Assembler.X86
{
    public static class CoreX86
    {
        public static readonly List<string> ASM = new();

        public static void Initialise()
        {
            ASM.Add("[org 0x7c00]");
            ASM.Add("jmp Source.Main\n");
        }

        public static void ParseMethod(MethodDef Method)
        {
            if (Method.Name == ".cctor")
                return;

            ASM.Add($"{Method.DeclaringType.Namespace}.{Method.Name}:");
            /*for (int i=0;i<method.Body.Variables.Count;i++) {
                var type=method.Body.Variables[i].Type;
                Console.WriteLine(type.ToString());
                code.Add($"{method.DeclaringType.Namespace}.{method.Name}.{method.Body.Variables[i].Name}:");
                code.Add($"\t.size dd {method.Body.Variables[i]}");
                //code.Add($"; {method.Body.Variables[i].Name} type: {method.Body.Variables[i].Type}");
                code.Add($"; {method.Body.Variables[i].Name} type: {method.Body.Variables[i].Type}");
            }*/
        }

        public static void ParseFields(TypeDef Type)
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

        public static string GetFieldName(TypeDef Class, FieldDef Var)
        {
            return $"{Class.Namespace}.{Class.Name}.{Var.Name}";
        }

        public static string GetMethodName(MethodDef Method)
        {
            return $"{GetTypeName(Method.DeclaringType)}.{Method.Name}";
        }

        public static string GetTypeName(TypeDef Def)
        {
            return $"{Def.Namespace}.{Def.Name}";
        }

        public static void Export(string Path)
        {
            ASM.Add("\ntimes 510-($-$$) db 0");
            ASM.Add("dw 0xAA55");
            string Final = "";
            foreach (string line in ASM)
            {
                Final += line + '\n';
            }
            File.WriteAllText(Path, Final);
        }
    }
}