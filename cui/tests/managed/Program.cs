using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cui.Managed;
using Raw = Managed.Forms.Cui;

internal static unsafe partial class Program
{
    private static int assertions;
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_ctx")]
    internal static extern ref Raw.cui_ctx* Context(Ui ui);
    internal static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
    internal static void Throws<T>(Action action, string message) where T : Exception
    {
        assertions++;
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + message);
    }
    private static int Main(string[] args)
    {
        try
        {
            if (args.Contains("--surface-audit")) SurfaceAudit();
            else if (args.Contains("--utf8")) Utf8Inputs();
            else if (args.Contains("--benchmark")) Benchmark();
            else Safety();
            Console.WriteLine($"PASS assertions={assertions} runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription} arch={System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Qualification roots the entire ManagedCui assembly; audit runs under JIT too.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Qualification roots the entire ManagedCui assembly.")]
    private static void SurfaceAudit()
    {
        var assembly = typeof(Ui).Assembly;
        var inspected = new HashSet<Type>();
        void Inspect(Type type)
        {
            if (type.IsByRef || type.IsArray) { Inspect(type.GetElementType()!); return; }
            Check(!type.IsPointer && !type.IsFunctionPointer && type != typeof(IntPtr) && type != typeof(UIntPtr), "Unsafe public type: " + type);
            Check(type.Assembly.GetName().Name is not "TranslatedCui" and not "Glfw", "Raw public type: " + type);
            if (type.IsGenericType) foreach (var arg in type.GetGenericArguments()) Inspect(arg);
            if (type.Assembly != assembly || !inspected.Add(type)) return;
            // CLR supplies every delegate's (object, IntPtr) constructor. The
            // user-callable callback contract is Invoke, including nested args.
            if (typeof(MulticastDelegate).IsAssignableFrom(type))
            {
                var invoke = type.GetMethod("Invoke")!; Inspect(invoke.ReturnType);
                foreach (var p in invoke.GetParameters()) Inspect(p.ParameterType);
                return;
            }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) Inspect(field.FieldType);
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                Inspect(method.ReturnType);
                foreach (var p in method.GetParameters()) Inspect(p.ParameterType);
            }
            foreach (var ctor in type.GetConstructors()) foreach (var p in ctor.GetParameters()) Inspect(p.ParameterType);
            foreach (var constraint in type.GetGenericArguments().Where(t => t.IsGenericParameter).SelectMany(t => t.GetGenericParameterConstraints())) Inspect(constraint);
        }
        foreach (var type in assembly.GetExportedTypes()) Inspect(type);
        Console.WriteLine($"SURFACE\ttypes={inspected.Count}\tunsafe=0");
    }
}
