using System.Security.Cryptography;
using System.Text;
using Managed.Zstd;

internal static class Program
{
    private static readonly byte[] Magic = "DZAR1\r\n\0"u8.ToArray();
    private static int Main(string[] args)
    {
        try
        {
            switch (args)
            {
                case ["--self-test", var directory]: SelfTest(directory); return 0;
                case ["pack", var source, var archive]: Pack(source, archive, null); return 0;
                case ["pack", var source, var archive, var dictionary]: Pack(source, archive, LoadDictionary(dictionary)); return 0;
                case ["unpack", var archive, var destination]: Unpack(archive, destination, null); return 0;
                case ["unpack", var archive, var destination, var dictionary]: Unpack(archive, destination, LoadDictionary(dictionary)); return 0;
                case ["train", var samples, var dictionary]: Train(samples, dictionary); return 0;
                default:
                    Console.WriteLine("Managed zstd archive: pack DIRECTORY ARCHIVE.zst [DICTIONARY] | unpack ARCHIVE.zst NEW_DIRECTORY [DICTIONARY] | train SAMPLE_DIRECTORY DICTIONARY | --self-test WORK");
                    return args.Length == 0 || args is ["--help"] ? 0 : 2;
            }
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static ZstdDictionary LoadDictionary(string path) => new(File.ReadAllBytes(path));
    private static void Train(string source, string destination)
    {
        var samples = Directory.GetFiles(source, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal)
            .Select(path => (ReadOnlyMemory<byte>)File.ReadAllBytes(path)).ToArray();
        var dictionary = ZstdDictionary.Train(samples);
        File.WriteAllBytes(destination, dictionary.ToArray());
        Console.WriteLine($"Trained {dictionary.Length} dictionary bytes from {samples.Length} records; id={dictionary.Id}.");
    }
    private static void Pack(string source, string archive, ZstdDictionary? dictionary)
    {
        string root = Path.GetFullPath(source);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);
        string target = Path.GetFullPath(archive);
        string prefix = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;
        if (target.StartsWith(prefix, StringComparison.Ordinal)) throw new ArgumentException("Write the archive outside the source directory.");
        var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        string staged = Path.GetTempFileName(); string temporaryArchive = target + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            using (var output = File.Create(staged))
            using (var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(Magic); writer.Write(files.Length);
                foreach (var file in files)
                {
                    byte[] name = Encoding.UTF8.GetBytes(Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/'));
                    using var input = File.OpenRead(file);
                    writer.Write(name.Length); writer.Write(name); writer.Write(input.Length); input.CopyTo(output);
                }
            }
            long bytes;
            using (var input = File.OpenRead(staged))
            using (var output = File.Create(temporaryArchive))
            using (var compressor = new ZstdCompressor(level: 9, dictionary))
            {
                bytes = input.Length; compressor.Compress(input, output, (ulong)input.Length);
            }
            File.Move(temporaryArchive, target, overwrite: true);
            Console.WriteLine($"Packed {files.Length} files, {bytes} archive bytes into {new FileInfo(target).Length} zstd bytes.");
        }
        finally { File.Delete(staged); File.Delete(temporaryArchive); }
    }
    private static void Unpack(string archive, string destination, ZstdDictionary? dictionary)
    {
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root) || File.Exists(root)) throw new IOException("Extraction requires a new destination directory.");
        string staged = Path.GetTempFileName();
        try
        {
            using (var input = File.OpenRead(archive))
            using (var output = File.Create(staged))
            using (var decoder = new ZstdDecompressor(dictionary)) decoder.Decompress(input, output);
            using var decoded = File.OpenRead(staged); using var reader = new BinaryReader(decoded, Encoding.UTF8, leaveOpen: true);
            if (!reader.ReadBytes(Magic.Length).AsSpan().SequenceEqual(Magic)) throw new InvalidDataException("Unrecognized archive format.");
            int count = reader.ReadInt32(); if (count is < 0 or > 1000000) throw new InvalidDataException("Invalid entry count.");
            Directory.CreateDirectory(root);
            for (int i = 0; i < count; i++)
            {
                int length = reader.ReadInt32(); if (length is <= 0 or > 32768) throw new InvalidDataException("Invalid filename length.");
                byte[] nameBytes = reader.ReadBytes(length); if (nameBytes.Length != length) throw new EndOfStreamException();
                string name = new UTF8Encoding(false, true).GetString(nameBytes);
                string[] parts = name.Split('/');
                if (parts.Any(part => part is "" or "." or ".." || part.IndexOfAny(['\\', ':', '\0']) >= 0) || Path.IsPathRooted(name))
                    throw new InvalidDataException("Invalid archive entry path.");
                string path = Path.Combine(root, Path.Combine(parts));
                long bytes = reader.ReadInt64(); if (bytes < 0 || bytes > decoded.Length - decoded.Position) throw new InvalidDataException("Invalid entry length.");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                byte[] buffer = new byte[131072];
                while (bytes != 0)
                {
                    int read = decoded.Read(buffer, 0, (int)Math.Min(bytes, buffer.Length));
                    if (read == 0) throw new EndOfStreamException(); output.Write(buffer, 0, read); bytes -= read;
                }
            }
            if (decoded.Position != decoded.Length) throw new InvalidDataException("Trailing archive bytes.");
            Console.WriteLine($"Restored {count} files to {root}.");
        }
        finally { File.Delete(staged); }
    }
    private static void SelfTest(string work)
    {
        Directory.CreateDirectory(work);
        string source = Path.Combine(work, "source"), samples = Path.Combine(work, "samples");
        Directory.CreateDirectory(Path.Combine(source, "nested")); Directory.CreateDirectory(samples);
        File.WriteAllText(Path.Combine(source, "readme.txt"), "Archive workflow through translated zstd: Žąsis 東京.\n" + new string('z', 130000));
        File.WriteAllBytes(Path.Combine(source, "nested", "payload.bin"), Enumerable.Range(0, 262147).Select(i => (byte)(i * 17 ^ i >> 8)).ToArray());
        File.WriteAllBytes(Path.Combine(source, "empty"), []);
        File.WriteAllText(Path.Combine(source, "nested", "duomenys-雪.txt"), "Multi-file restore keeps Unicode names and content.\n");
        for (int i = 0; i < 256; i++) File.WriteAllText(Path.Combine(samples, i.ToString("D4") + ".txt"),
            string.Join('\n', Enumerable.Range(0, 24).Select(j => $"Archive workflow through translated zstd: Žąsis 東京; record={i};field={j};kind={i % 17};message=Multi-file restore keeps Unicode names and content.")));
        string dictionaryPath = Path.Combine(work, "archive.dict"); Train(samples, dictionaryPath); var dictionary = LoadDictionary(dictionaryPath);
        foreach (bool useDictionary in new[] { false, true })
        {
            string suffix = useDictionary ? "dictionary" : "plain";
            string frame = Path.Combine(work, suffix + ".dzar.zst"), restored = Path.Combine(work, "restored-" + suffix);
            Pack(source, frame, useDictionary ? dictionary : null); Unpack(frame, restored, useDictionary ? dictionary : null);
            var names = Directory.GetFiles(source, "*", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(source, path)).Order(StringComparer.Ordinal).ToArray();
            var actual = Directory.GetFiles(restored, "*", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(restored, path)).Order(StringComparer.Ordinal).ToArray();
            if (!names.SequenceEqual(actual)) throw new InvalidDataException("Restored filename inventory differs.");
            foreach (string name in names)
            {
                using var expectedFile = File.OpenRead(Path.Combine(source, name)); using var actualFile = File.OpenRead(Path.Combine(restored, name));
                if (!SHA256.HashData(expectedFile).AsSpan().SequenceEqual(SHA256.HashData(actualFile))) throw new InvalidDataException("Restored file differs: " + name);
            }
        }
        Console.WriteLine($"passed:archive-workflow;version={ZstdCodec.Version};files=4;frames=2;dictionary={dictionary.Id};streaming=true");
    }
}
