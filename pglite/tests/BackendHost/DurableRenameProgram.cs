using System.Text;
using Core = Managed.PGlite.Initialization.Translated.Initdb;

internal static unsafe class DurableRenameProgram
{
    public static int Main(string[] args)
    {
        string root = args[0]; Directory.CreateDirectory(root);
        for (int ownerIndex = 0; ownerIndex < 2; ownerIndex++)
        {
            using var owner = Core.__DotCcCreateContext(); using var binding = owner.Enter();
            string directory = Path.Combine(root, "owner-" + ownerIndex); Directory.CreateDirectory(directory);
            string source = Path.Combine(directory, "source"), target = Path.Combine(directory, "target");
            File.WriteAllText(source, "original complete PostgreSQL durable_rename UTF-8 Ω");
            File.WriteAllText(target, "previous bytes");
            fixed (byte* oldName = Encoding.UTF8.GetBytes(source + "\0"))
            fixed (byte* newName = Encoding.UTF8.GetBytes(target + "\0"))
            fixed (byte* parent = Encoding.UTF8.GetBytes(directory + "\0"))
            {
                // Establish the real capability first: upstream's portable
                // fsync_fname is allowed to skip EISDIR on some platforms.
                int descriptor = Core.Libc.open(parent, 0);
                if (descriptor < 0) throw new Exception("Actual directory open unavailable: " + Core.Libc.errno);
                try { if (Core.Libc.fsync(descriptor) != 0) throw new Exception("Actual directory fsync failed: " + Core.Libc.errno); }
                finally { Core.Libc.close(descriptor); }
                if (Core.fsync_fname(parent, true) != 0 || Core.fsync_parent_path(newName) != 0)
                    throw new Exception("Original directory/parent synchronization failed");
                if (Core.durable_rename(oldName, newName) != 0) throw new Exception("Original replacement durable_rename failed");
                if (File.Exists(source) || File.ReadAllText(target) != "original complete PostgreSQL durable_rename UTF-8 Ω")
                    throw new Exception("Original durable_rename lost bytes or source lifetime");
                File.WriteAllText(source, "new target path"); File.Delete(target);
                if (Core.durable_rename(oldName, newName) != 0 || File.Exists(source) || File.ReadAllText(target) != "new target path")
                    throw new Exception("Original durable_rename absent-target path failed");
            }
        }
        Console.WriteLine("PASS original complete frontend fsync_fname/fsync_parent_path/durable_rename; two owners, existing and absent targets");
        return 0;
    }
}
