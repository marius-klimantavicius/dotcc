using System;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

public static class ProofHost
{
    public static int Main()
    {
        var original = typeof(DotCcLib).GetMethods().Single(method => method.Name.StartsWith("original__encode", StringComparison.Ordinal)).GetParameters()[2].ParameterType;
        var patched = typeof(DotCcLib).GetMethods().Single(method => method.Name.StartsWith("patched__encode", StringComparison.Ordinal)).GetParameters()[2].ParameterType;
        string[] names = ["rows", "columns", "cell_width", "cell_height"];
        Type[] fields = [typeof(ushort), typeof(ushort), typeof(uint), typeof(uint)];
        if (Marshal.SizeOf(original) != 12 || Marshal.SizeOf(patched) != 12) return 20;
        var offsets = new int[4];
        for (int i = 0; i < names.Length; i++)
        {
            if (original.GetField(names[i]).FieldType != fields[i] || patched.GetField(names[i]).FieldType != fields[i]) return 21;
            offsets[i] = (int)Marshal.OffsetOf(patched, names[i]);
            if ((int)Marshal.OffsetOf(original, names[i]) != offsets[i]) return 22;
        }
        Console.WriteLine("MANAGED_LAYOUT size=12 offsets=" + string.Join(",", offsets));
        return DotCcLib.answer();
    }
}
