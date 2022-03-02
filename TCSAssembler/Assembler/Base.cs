using dnlib.DotNet;

namespace TCSAssembler.Assembler
{
    public abstract class Base
    {
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
    }
}