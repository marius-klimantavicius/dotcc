namespace DotCC;

// Compiler-owned storage only. A tracked interior reference survives GC; no C
// value is copied here and no raw pointer is retained over a call/safepoint.
internal static class GlobalAddressCache
{
    internal const string TypeName = "__DotccGlobalAddressCache";
    internal const string Declaration = """
        internal unsafe ref struct __DotccGlobalAddressCache
        {
            private ref byte value;
            internal T* Get<T>(delegate* managed<ref T> acquire) where T : unmanaged
            {
                if (global::System.Runtime.CompilerServices.Unsafe.IsNullRef(ref value))
                    value = ref global::System.Runtime.CompilerServices.Unsafe.As<T, byte>(ref acquire());
                return (T*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref value);
            }
        }
        """ + "\n";

    internal static string Local(string type, string name, string storage) =>
        TypeName + " " + name + " = default;\nstatic ref " + type + " " + name + "_Get() => ref " + storage + ";\n";
    internal static string Use(string type, string name) => name + ".Get<" + type + ">(&" + name + "_Get)";
}
