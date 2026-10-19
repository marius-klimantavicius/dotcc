using System.Text;

internal static unsafe class OwnerEntry
{
    private static int Main(string[] args)
    {
        if (args.Length != 2) return 20;
        string processDirectory = Directory.GetCurrentDirectory();
        using var owner = new Libc.RuntimeContext();
        using var binding = owner.Enter();
        byte[] path = Encoding.UTF8.GetBytes(Path.GetFullPath(args[0]) + "\0");
        fixed (byte* pointer = path)
            if (Libc.chdir(pointer) != 0) return 21;
        if (Directory.GetCurrentDirectory() != processDirectory) return 22;
        int result = DotCcProgram.symlink_control_r37();
        byte[] link = Encoding.UTF8.GetBytes("link\0");
        byte* target = stackalloc byte[16];
        fixed (byte* pointer = link)
            if (Libc.readlink(pointer, target, 16) != 6) return 23;
        string ownerLink = Path.Combine(args[0], "link");
        if (new FileInfo(ownerLink).LinkTarget != "target") return 24;
        // A different explicit owner starts in the actual process directory.
        // It must not observe the first owner's virtual cwd or relative link.
        using (var other = new Libc.RuntimeContext())
        using (other.Enter())
        {
            byte[] otherPath = Encoding.UTF8.GetBytes(Path.GetFullPath(args[1]) + "\0");
            fixed (byte* pointer = otherPath)
                if (Libc.chdir(pointer) != 0) return 25;
            fixed (byte* pointer = link)
                if (Libc.readlink(pointer, target, 16) != -1) return 26;
        }
        fixed (byte* pointer = link)
            if (Libc.readlink(pointer, target, 16) != 6) return 27;
        Console.WriteLine("explicit owner relative symlink target/cwd/restoration verified");
        return result;
    }
}
