using System.Runtime.InteropServices;
using VM = Managed.Interpreters.CPython;

unsafe
{
    string[] fields = ["st_dev", "st_ino", "st_mode", "st_nlink", "st_uid", "st_gid", "st_size", "st_atime", "st_mtime", "st_ctime", "st_rdev", "st_blksize", "st_blocks"];
    int[] expected = [0, 8, 16, 24, 32, 36, 40, 48, 56, 64, 72, 80, 88];
    if (sizeof(VM.stat) != 96) throw new Exception("Actual emitted stat size");
    for (int i = 0; i < fields.Length; i++)
        if (Marshal.OffsetOf<VM.stat>(fields[i]).ToInt32() != expected[i]) throw new Exception(fields[i]);
    Console.WriteLine("actual emitted stat: size=96 offsets=" + string.Join(",", expected));
    Console.WriteLine(typeof(VM.stat).Assembly.Location + " MVID=" + typeof(VM.stat).Assembly.ManifestModule.ModuleVersionId);
}
