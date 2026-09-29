using System.Runtime.InteropServices;
using Managed.Networking.Tools;

unsafe
{
    var strings = new nint[args.Length + 1];
    byte** argv = (byte**)NativeMemory.AllocZeroed((nuint)(strings.Length + 1), (nuint)sizeof(nint));
    try
    {
        strings[0] = Marshal.StringToCoTaskMemUTF8("makefsdata");
        for (int i = 0; i < args.Length; i++) strings[i + 1] = Marshal.StringToCoTaskMemUTF8(args[i]);
        for (int i = 0; i < strings.Length; i++) argv[i] = (byte*)strings[i];
        using var tool = new MakeFsData();
        using var binding = tool.__DotCcEnter();
        return tool.main(strings.Length, argv);
    }
    finally
    {
        foreach (nint text in strings) if (text != 0) Marshal.FreeCoTaskMem(text);
        NativeMemory.Free(argv);
    }
}
