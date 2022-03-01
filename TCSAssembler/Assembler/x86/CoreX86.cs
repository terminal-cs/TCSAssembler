namespace TCSAssembler.Assembler.X86
{
    public static class CoreX86
    {
        public static int VI { get; set; }
        public static Dictionary<string, List<string>> Methods { get; set; } = new();

        public static void NewMethod(string MethodName)
        {
            Methods.Add(MethodName, new());
        }

        public static void NewASM(string MethodName, string ASM)
        {
            Methods[MethodName].Add(ASM);
        }

        public static void NewString(string Name, string Contents)
        {
            NewASM("Strings", Name + ": db '" + Contents + "', 0");
        }

        public static void Init()
        {
            NewMethod("Main");
            NewMethod("Strings");
        }

        public static void Export(string Path)
        {
            string Final = "[org 0x7c00]\njmp Main\n";
            foreach (KeyValuePair<string, List<string>> Method in Methods)
            {
                NewASM(Method.Key, "ret");
                Final += "\n" + Method.Key + ":\n";
                foreach (string Contents in Method.Value)
                {
                    Final += "  " + Contents + "\n";
                }
            }
            Final += "\ntimes 510-($-$$) db 0\ndw 0xAA55";
            File.WriteAllText(Path, Final);
        }
    }
}