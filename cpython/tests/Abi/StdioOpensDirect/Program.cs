using System.Text;
using static DotCC.Libc.Libc;

unsafe class Program
{
    static void Phase(string kind, string path) { Console.WriteLine($"phase {kind} {path}"); Console.Out.Flush(); Console.ReadLine(); }
    static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--checks")
        {
            Console.WriteLine("stdio-open-direct checks=" + DotCC.Tests.CPythonR10StdioOpenChecks.RunAll());
            return 0;
        }
        using var owner = new RuntimeContext();
        using var scope = owner.Enter();
        fixed(byte* path=Encoding.UTF8.GetBytes(args[0]+"\0"), mode=Encoding.UTF8.GetBytes("r+\0"),
                    template=Encoding.UTF8.GetBytes(args[1]+"\0"))
        {
            FILE* f=fopen(path,mode);
            if (f==null) throw new Exception("fopen errno="+errno);
            Phase("fopen",args[0]);
            if(fclose(f)!=0) throw new Exception("close");
            f=fopen(path,mode);
            if(f==null || freopen(path,mode,f)!=f) throw new Exception("freopen errno="+errno);
            Phase("freopen",args[0]);
            fclose(f);
            int fd=mkstemp(template);
            if(fd<0) throw new Exception("mkstemp errno="+errno);
            string name=MarshalString(template);
            Phase("mkstemp",name);
            close(fd);
            Phase("closed",name);
            unlink(template);
        }
        Console.WriteLine("probe complete");
        return 0;
    }
    static string MarshalString(byte* p) => System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)p)!;
}
