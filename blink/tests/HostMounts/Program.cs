using System.Text;
using Managed.Emulation.Host;

static void Check(bool value, string message) { if (!value) throw new Exception(message); }
static T Value<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new Exception("Unexpected filesystem result: " + result.Error);
static VirtualFileSystem Memory(long limit = 4096) => new(new Dictionary<string, ReadOnlyMemory<byte>>(), writableLimit: limit);
static void Write(IGuestFileSystem fs, string path, string text)
{
    int fd = Value(fs.Open(path, FileAccessMode.Write, create: true, truncate: true));
    try { Check(Value(fs.Write(fd, Encoding.UTF8.GetBytes(text))) == text.Length, "write length"); }
    finally { Value(fs.Close(fd)); }
}
static string Read(IGuestFileSystem fs, string path)
{
    int fd = Value(fs.Open(path, FileAccessMode.Read));
    try { byte[] bytes = new byte[256]; int n = Value(fs.Read(fd, bytes)); return Encoding.UTF8.GetString(bytes, 0, n); }
    finally { Value(fs.Close(fd)); }
}
string scratch = Path.GetFullPath(args.Single()); Directory.CreateDirectory(scratch);
string livePath = Path.Combine(scratch, "live"), nestedPath = Path.Combine(scratch, "nested"), exportedPath = Path.Combine(scratch, "exported");
Directory.CreateDirectory(livePath); Directory.CreateDirectory(nestedPath); Directory.CreateDirectory(exportedPath);
File.WriteAllText(Path.Combine(livePath, "input"), "base"); File.WriteAllText(Path.Combine(nestedPath, "input"), "nested");
using var live = HostDirectoryFileSystem.OpenRoot(livePath, writableLimit: 4096);
using var readOnly = HostDirectoryFileSystem.OpenRoot(livePath, readOnly: true, writableLimit: 4096);
using var nested = HostDirectoryFileSystem.OpenRoot(nestedPath, writableLimit: 4096);
using var root = Memory(); Value(root.MakeDirectory("/work", 0x1ed)); Write(root, "/work/hidden", "underlay");
using var machine = new MountedFileSystem(root, privateWritableLimit: 4096);
bool overlayRejected = false; try { machine.Mount("/work", live); } catch (ArgumentException) { overlayRejected = true; }
Check(overlayRejected, "overlay must be explicit");
machine.Mount("/work", live, replaceExistingDirectory: true);
machine.Mount("/work/nested", nested);
machine.Mount("/ro", readOnly, readOnly: true);
Check(!machine.Stat("/work/hidden").Succeeded, "overlay hides existing contents");
Check(Read(machine, "/work/nested/input") == "nested", "nested precedence");
Check(machine.Rename("/work/input", "/work/nested/renamed").Error == GuestError.CrossDevice, "cross mount rename");
Check(machine.Open("/ro/input", FileAccessMode.Write).Error == GuestError.ReadOnly, "read only grant");
Check(machine.Stat("/work/../../outside").Error == GuestError.Access, "namespace root escape denied");
bool duplicateRejected = false; try { machine.Mount("/work", nested, replaceExistingDirectory: true); } catch (ArgumentException) { duplicateRejected = true; }
Check(duplicateRejected, "duplicate mount");
using (var session = machine.AcquireSession())
{
    bool frozen = false; try { machine.Unmount("/ro"); } catch (InvalidOperationException) { frozen = true; }
    Check(frozen, "namespace frozen during execution");
    await using var io = new InstanceIo(session.FileSystem);
    Value(io.ChangeDirectory("/work"));
    int fd = Value(io.OpenFile("result", FileAccessMode.Read | FileAccessMode.Write, create: true));
    Check(Value(io.WriteAt(fd, "abc"u8, 0)) == 3, "live write");
    Check(File.ReadAllText(Path.Combine(livePath, "result")) == "abc", "host observes live guest write");
    Write(session.FileSystem, "/persist", "first");
}
Check(machine.OpenDescriptors == 0 && live.OpenDescriptors == 0, "session closes file handles");
File.WriteAllText(Path.Combine(livePath, "result"), "host-edit");
using (var session = machine.AcquireSession())
{
    Check(Read(session.FileSystem, "/persist") == "first", "private data persists across runs");
    Check(Read(session.FileSystem, "/work/result") == "host-edit", "live host changes remain visible");
    int directory = Value(session.FileSystem.Open("/work", FileAccessMode.Read, allowDirectory: true, requireDirectory: true));
    var entries = Value(session.FileSystem.DirectorySnapshot(directory, 32, 4096));
    Check(entries.Count(e => e.Name == "nested") == 1 && entries.All(e => e.Name != "hidden"), "mount enumeration");
    Value(session.FileSystem.Close(directory));
}
machine.Unmount("/work/nested"); machine.Unmount("/work");
Check(Read(machine, "/work/hidden") == "underlay", "unmount restores contents");
Check(File.ReadAllText(Path.Combine(livePath, "result")) == "host-edit", "unmount preserves live writes");
using var cow = Memory(); var copied = GuestFileSystemCopy.Copy(readOnly, cow, 4096);
Check(copied.Files == 2 && copied.Bytes == 13, "bounded eager import costs");
File.WriteAllText(Path.Combine(livePath, "input"), "changed");
Check(Read(cow, "/input") == "base", "COW import is frozen"); Write(cow, "/input", "private");
Check(File.ReadAllText(Path.Combine(livePath, "input")) == "changed", "COW never updates host source");
using var export = HostDirectoryFileSystem.OpenRoot(exportedPath, writableLimit: 4096);
GuestFileSystemCopy.Copy(cow, export, 4096); Check(File.ReadAllText(Path.Combine(exportedPath, "input")) == "private", "explicit export");
GuestFileSystemCopy.Clear(cow); Check(!cow.Stat("/input").Succeeded, "explicit discard");

using var smallRoot = Memory(100); using var smallCow = Memory(100);
using var quota = new MountedFileSystem(smallRoot, privateWritableLimit: 8);
quota.Mount("/cow", smallCow, privateStorage: true);
Write(quota, "/root", "1234"); Write(quota, "/cow/file", "5678");
int limited = Value(quota.Open("/cow/file", FileAccessMode.Write, append: true));
Check(quota.Write(limited, "9"u8).Error == GuestError.NoSpace, "aggregate COW/root growth cap");
Check(quota.Truncate("/root", 5).Error == GuestError.NoSpace, "aggregate truncate cap");
Value(quota.Close(limited)); Value(quota.Truncate("/root", 2));
int extend = Value(quota.Open("/cow/file", FileAccessMode.Write, append: true));
Check(Value(quota.Write(extend, "abc"u8)) == 2, "bounded partial growth"); Value(quota.Close(extend));
Check(Read(quota, "/cow/file") == "5678ab", "partial write bytes");

using var nodeRoot = Memory(); using var nodeCow = Memory();
using var nodes = new MountedFileSystem(nodeRoot, privateNodeLimit: 4);
nodes.Mount("/cow", nodeCow, privateStorage: true); // two persistent store roots
Write(nodes, "/one", "1"); Value(nodes.MakeDirectory("/cow/two", 0x1ed));
Check(nodes.Open("/cow/third", FileAccessMode.Write, create: true).Error == GuestError.NoSpace, "aggregate private file node cap");
Check(nodes.MakeDirectory("/third", 0x1ed).Error == GuestError.NoSpace, "aggregate private directory node cap");
Value(nodes.Unlink("/one")); Write(nodes, "/cow/third", "3");
Check(nodes.NodeCount == 4, "released node capacity is reusable");

// Normal file-description semantics, independently mirrored by probe.c.
Write(live, "/normal", "abcdef"); int opened = Value(live.Open("/normal", FileAccessMode.Read | FileAccessMode.Write));
int duplicate = Value(live.Duplicate(opened)); Value(live.Seek(opened, 2, SeekOrigin.Begin)); byte[] pair = new byte[2];
Check(Value(live.Read(duplicate, pair)) == 2 && Encoding.ASCII.GetString(pair) == "cd", "dup shares offset");
Check(Value(live.Seek(opened, 0, SeekOrigin.Current)) == 4, "shared offset advanced");
Value(live.WriteAt(opened, "XY"u8, 0)); Value(live.SetAppend(duplicate, true)); Value(live.Write(opened, "!"u8));
Value(live.Rename("/normal", "/renamed")); Value(live.Unlink("/renamed"));
Check(Value(live.ReadAt(opened, pair, 0)) == 2 && Encoding.ASCII.GetString(pair) == "XY", "unlinked open description");
Value(live.Close(opened)); Value(live.Close(duplicate));
Check(live.OpenDescriptors == 0, "all descriptions closed");
if (!OperatingSystem.IsWindows())
{
    // Ordinary policy denial, not invalid access or fault injection.
    string link = Path.Combine(livePath, "link"); File.CreateSymbolicLink(link, Path.Combine(nestedPath, "input"));
    Check(live.Stat("/link").Error == GuestError.Access, "symlink policy"); File.Delete(link);
}
using (var reopened = HostDirectoryFileSystem.OpenRoot(livePath, writableLimit: 4096))
    Check(Read(reopened, "/result") == "host-edit", "fresh backend sees persistent host data");
Console.WriteLine("normal file descriptions: PASS");
