/*namespace TCSAssembler.Assembler.X86
{
    public static class Console
    {
        public static void Write(char Char)
        {
            CoreX86.NewASM("Main", "mov al, '" + Char + "'");
            CoreX86.NewASM("Main", "int 0x10");
        }

        public static void Write(string String)
        {
            String = String.Replace("'", "\\'").Replace("\n", "\\n");
            string VName = "V" + CoreX86.VI++;
            CoreX86.NewString(VName, String);
            CoreX86.NewASM("Main", "mov al, [" + VName + "]");
            CoreX86.NewASM("Main", "int 0x10");
        }

        public static void WriteLine(string String)
        {
            Write(String + "\n");
        }
    }
}*/