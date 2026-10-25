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

// Each independent open retains the same inode once; dup retains its open
// description (and offset) until the last duplicate closes.
foreach (bool mountedRoot in new[] { false, true })
{
    string retainedPath = Directory.CreateDirectory(Path.Combine(scratch, mountedRoot ? "retained-mounted" : "retained")).FullName;
    var retainedStore = HostDirectoryFileSystem.OpenRoot(retainedPath, writableLimit: 100, nodeLimit: 4);
    using IGuestFileSystem retained = mountedRoot
        ? new MountedFileSystem(retainedStore, ownsRoot: true, privateWritableLimit: 100, privateNodeLimit: 4)
        : retainedStore;
    int first = Value(retained.Open("/file", FileAccessMode.Read | FileAccessMode.Write, create: true));
    Check(Value(retained.Write(first, new byte[60])) == 60, "initial retained bytes");
    int second = Value(retained.Open("/file", FileAccessMode.Read | FileAccessMode.Write));
    int dup = Value(retained.Duplicate(first));
    Check(Value(retained.Seek(second, 0, SeekOrigin.Current)) == 0 && Value(retained.Seek(dup, 0, SeekOrigin.Current)) == 60,
        "separate open offsets differ from dup offset");
    if (HostDirectoryFileSystem.SupportsPersistentUnixModes) Value(retained.ChangeMode(first, 0x1a4)); // 0644
    var access = new VirtualFileTimeUpdate(FileTimeSelection.Explicit, new(1700000000, 0));
    var modify = new VirtualFileTimeUpdate(FileTimeSelection.Explicit, new(1700000010, 0));
    Value(retained.SetTimes(first, access, modify));
    var before = Value(retained.FStat(first));
    Check(before.Inode == Value(retained.FStat(second)).Inode && before.Links == 1, "independent opens share linked inode");
    Value(retained.Unlink("/file"));
    var after = Value(retained.FStat(second));
    Check(after.Inode == before.Inode && after.Mode == before.Mode && after.Immutable == before.Immutable &&
        after.AccessTicks == before.AccessTicks && after.ModifyTicks == before.ModifyTicks &&
        after.ChangeTicks >= before.ChangeTicks && after.Links == 0 && after.Length == 60,
        "unlink preserves inode metadata and removes link");
    var capacity = Value(retained.Capacity(second));
    Check(retained.WritableBytes == 60 && retained.NodeCount == 2 && capacity.FreeBytes == 40 && capacity.FreeNodes == 2,
        "retained inode is charged once across independent opens and dup");
    Check(Value(retained.WriteAt(second, "x"u8, 0)) == 1, "detached overwrite within quota");
    var written = Value(retained.FStat(dup));
    Check(written.ModifyTicks > before.ModifyTicks && written.ChangeTicks >= after.ChangeTicks, "detached write updates metadata across descriptions");
    Value(retained.Truncate(dup, 70));
    Check(Value(retained.FStat(second)).Length == 70 && retained.WritableBytes == 70, "detached truncate is visible to all opens");
    if (HostDirectoryFileSystem.SupportsPersistentUnixModes)
    {
        Value(retained.ChangeMode(second, 0x180));
        Check(Value(retained.FStat(dup)).Mode == 0x8180, "detached chmod updates shared inode");
    }
    Value(retained.SetTimes(second, access, modify));
    var updated = Value(retained.FStat(dup));
    Check(updated.AccessTicks == access.Time.Ticks && updated.ModifyTicks == modify.Time.Ticks && updated.Links == 0,
        "detached descriptor times are preserved across independent opens");
    Write(retained, "/file", "new");
    Check(Value(retained.Stat("/file")).Inode != before.Inode && Value(retained.FStat(first)).Length == 70,
        "recreated path has independent identity and contents");
    Value(retained.Rename("/file", "/fresh")); Value(retained.Unlink("/fresh"));
    Check(Value(retained.FStat(second)).Inode == before.Inode && Value(retained.FStat(second)).Length == 70,
        "later path rename and unlink cannot retarget detached inode");
    Value(retained.Close(first));
    Check(retained.WritableBytes == 70 && retained.NodeCount == 2, "dup still retains original description");
    Value(retained.Close(dup));
    Check(retained.WritableBytes == 70 && retained.NodeCount == 2, "independent description retains storage after last dup");
    Value(retained.Close(second));
    Check(retained.WritableBytes == 0 && retained.NodeCount == 1 && Value(retained.Capacity("/")).FreeBytes == 100,
        "final independent close releases retained bytes and node");

    Write(retained, "/source", "source"); Write(retained, "/target", "target-old");
    int source = Value(retained.Open("/source", FileAccessMode.Read));
    int target = Value(retained.Open("/target", FileAccessMode.Read | FileAccessMode.Write));
    int otherTarget = Value(retained.Open("/target", FileAccessMode.Read));
    int targetDup = Value(retained.Duplicate(target));
    Value(retained.SetTimes(target, access, modify));
    var oldTarget = Value(retained.FStat(target)); var oldSource = Value(retained.FStat(source));
    Value(retained.Rename("/source", "/target"));
    var replaced = Value(retained.FStat(otherTarget)); var replacement = Value(retained.Stat("/target"));
    Check(replaced.Inode == oldTarget.Inode && replaced.Mode == oldTarget.Mode && replaced.ModifyTicks == oldTarget.ModifyTicks &&
        replaced.AccessTicks == oldTarget.AccessTicks && replaced.Links == 0 && replaced.Length == 10,
        "replacement preserves old target inode metadata");
    Check(replacement.Inode == oldSource.Inode && Value(retained.FStat(source)).Links == 1 &&
        Read(retained, "/target") == "source", "replacement moves source inode and path");
    Check(retained.WritableBytes == 16 && retained.NodeCount == 3 && Value(retained.Capacity("/")).FreeBytes == 84,
        "replacement charges detached target once");
    Check(Value(retained.WriteAt(target, "!"u8, 0)) == 1 && Read(retained, "/target") == "source",
        "writing replaced inode does not alter replacement");
    Value(retained.Close(target)); Value(retained.Close(otherTarget));
    Check(retained.WritableBytes == 16, "duplicate retains replaced inode");
    Value(retained.Close(targetDup));
    Check(retained.WritableBytes == 6 && retained.NodeCount == 2, "final replacement close releases old target");
    Value(retained.Close(source)); Value(retained.Unlink("/target"));

    Value(retained.MakeDirectory("/directory", 0x1ed));
    int directory = Value(retained.Open("/directory", FileAccessMode.Read, allowDirectory: true));
    int otherDirectory = Value(retained.Open("/directory", FileAccessMode.Read, allowDirectory: true));
    Value(retained.SetTimes(directory, access, modify)); var directoryBefore = Value(retained.FStat(directory));
    Value(retained.Unlink("/directory", removeDirectory: true));
    var detachedDirectory = Value(retained.FStat(otherDirectory));
    Check(detachedDirectory.Directory && detachedDirectory.Inode == directoryBefore.Inode && detachedDirectory.Mode == directoryBefore.Mode &&
        detachedDirectory.ModifyTicks == directoryBefore.ModifyTicks && detachedDirectory.Links == 0 && retained.NodeCount == 2,
        "open removed directory preserves metadata and retains one node");
    Value(retained.Close(directory)); Check(retained.NodeCount == 2, "other directory open retains node");
    Value(retained.Close(otherDirectory)); Check(retained.NodeCount == 1, "final directory close releases node");
}
if (!OperatingSystem.IsWindows())
{
    // Ordinary policy denial, not invalid access or fault injection.
    string link = Path.Combine(livePath, "link"); File.CreateSymbolicLink(link, Path.Combine(nestedPath, "input"));
    Check(live.Stat("/link").Error == GuestError.Access, "symlink policy"); File.Delete(link);
}
using (var reopened = HostDirectoryFileSystem.OpenRoot(livePath, writableLimit: 4096))
    Check(Read(reopened, "/result") == "host-edit", "fresh backend sees persistent host data");
Console.WriteLine("normal file descriptions: PASS");
