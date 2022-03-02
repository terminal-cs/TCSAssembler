using dnlib.DotNet;

namespace TCSAssembler.Assembler.X86
{
    public static class CoreX86
    {
        static List<string> code=new List<string>();
        public static void Initialise() {
            code.Add("[org 0x7c00]");
            code.Add("jmp KernelEntry");
        }
        public static void ParseMethod(MethodDef method) {
            if (method.Name==".cctor")
                return;
            code.Add($"; {method.Name} inside of ${method.DeclaringType.Namespace}");
            if (method.GetParamCount()>0)
                code.Add($"; first param: {method.GetParam(0).FullName}");
            code.Add($"{method.DeclaringType.Namespace}.{method.Name}:");
            /*for (int i=0;i<method.Body.Variables.Count;i++) {
                var type=method.Body.Variables[i].Type;
                Console.WriteLine(type.ToString());
                code.Add($"{method.DeclaringType.Namespace}.{method.Name}.{method.Body.Variables[i].Name}:");
                code.Add($"\t.size dd {method.Body.Variables[i]}");
                //code.Add($"; {method.Body.Variables[i].Name} type: {method.Body.Variables[i].Type}");
                code.Add($"; {method.Body.Variables[i].Name} type: {method.Body.Variables[i].Type}");
            }*/
        }
        public static void ParseFields(TypeDef type)
        {
            foreach (var variable in type.Fields)
            {
                //foreach variable in class :p (like int test in the kernel class (kernel.cs))
                if (variable.IsStatic)
                {
                    //if the variable is static
                    code.Add($"{GetFieldName(type, variable)}: dq {(variable.HasConstant?variable.Constant.Value+$" ;type: {variable.Constant.Type}":0)}");
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
        public static string GetFieldName(TypeDef _class, FieldDef _var) {
            return $"{_class.Namespace}.{_class.Name}.{_var.Name}";
        }
        public static string GetMethodName(MethodDef method) {
            return $"{GetTypeName(method.DeclaringType)}.{method.Name}";
        }
        public static string GetTypeName(TypeDef def)
        {
            return $"{def.Namespace}.{def.Name}";
        }

        public static void Export(string Path)
        {
            code.Add("times 510-($-$$) db 0");
            code.Add("dw 0xAA55");
            string Final="";
            foreach (string line in code) {
                Final+=$"{line}\n";
            }
            File.WriteAllText(Path, Final);
        }
    }
}