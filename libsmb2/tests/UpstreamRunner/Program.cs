using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

if (args is ["--help"] or ["-h"])
{
    Console.WriteLine("UpstreamRunner --manifest <manifest.json>");
    return 0;
}
if (args is not ["--manifest", var manifestPath])
{
    Console.Error.WriteLine("Usage: UpstreamRunner --manifest <manifest.json>");
    return 2;
}
try
{
    var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(manifestPath), Json.Options)
        ?? throw new InvalidDataException("Empty manifest");
    return await new Runner(manifest).Run();
}
catch (Exception error)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}

sealed class Manifest
{
    public string SourceRoot { get; set; } = "";
    public string ArtifactRoot { get; set; } = "";
    public string TestUrl { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 120;
    public Dictionary<string, string> Environment { get; set; } = [];
    public Dictionary<string, JsonElement> BaselineBlockedPrograms { get; set; } = [];
    public List<Variant> Variants { get; set; } = [];
}
sealed class Variant
{
    public string Name { get; set; } = "";
    public string? TestUrl { get; set; }
    public Dictionary<string, string> Environment { get; set; } = [];
    public Dictionary<string, string[]> Programs { get; set; } = [];
}
sealed class CaseReceipt(string name)
{
    public string Name { get; } = name;
    public string Status { get; set; } = "running";
    public string? Reason { get; set; }
    public JsonElement? BaselineEvidence { get; set; }
    public List<string> Checks { get; } = [];
    public List<Invocation> Invocations { get; } = [];
    public Dictionary<string, string> SourceSha256 { get; } = [];
    public Dictionary<string, string> FixtureSha256 { get; } = [];
    public List<string> Adaptations { get; } = [];
}
sealed record Invocation(string Program, int ExitCode, bool TimedOut, long DurationMs,
    string Stdout, string Stderr, string StdoutSha256, string StderrSha256);
sealed record ProcessResult(int ExitCode, bool TimedOut, byte[] Stdout, byte[] Stderr);
sealed record VariantReceipt(string Name, List<CaseReceipt> Cases);
static class Json
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}

sealed class Runner(Manifest manifest)
{
    readonly List<VariantReceipt> results = [];
    string source = "", artifacts = "";
    Variant variant = null!;
    CaseReceipt current = null!;
    string directory = "";

    public async Task<int> Run()
    {
        source = Path.GetFullPath(manifest.SourceRoot);
        artifacts = Path.GetFullPath(manifest.ArtifactRoot);
        if (!Directory.Exists(Path.Combine(source, "tests")))
            throw new InvalidDataException("sourceRoot must be the pinned upstream source directory");
        if (manifest.Variants.Count == 0 || manifest.TimeoutSeconds <= 0)
            throw new InvalidDataException("At least one variant and a positive timeoutSeconds are required");
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in manifest.Variants)
            if (!Regex.IsMatch(item.Name, "^[a-zA-Z0-9_-]+$") || !names.Add(item.Name))
                throw new InvalidDataException("Variant names must be unique safe directory names");
        Directory.CreateDirectory(artifacts);
        foreach (var item in manifest.Variants)
        {
            variant = item;
            var cases = new List<CaseReceipt>();
            results.Add(new(item.Name, cases));
            foreach (string path in Directory.GetFiles(Path.Combine(source, "tests"), "test_*.sh").Order())
            {
                string name = Path.GetFileName(path);
                current = CreateCase(name);
                cases.Add(current);
                directory = Path.Combine(artifacts, item.Name, name);
                Directory.CreateDirectory(directory);
                try
                {
                    await RunShellCase(name);
                    if (current.Status == "running") current.Status = "passed";
                }
                catch (Exception error) { current.Status = "failed"; current.Reason = error.Message; }
                Console.WriteLine($"{item.Name}: {name}: {current.Status}");
            }
            foreach (string program in new[] { "aes128ccm-test", "ntlmssp_generate_blob" })
            {
                current = CreateCase($"{program}.c");
                cases.Add(current);
                directory = Path.Combine(artifacts, item.Name, program);
                Directory.CreateDirectory(directory);
                try
                {
                    if (Require(program))
                    {
                        var result = await Success(program);
                        if (program == "aes128ccm-test")
                        {
                            var matches = Regex.Matches(Encoding.UTF8.GetString(result.Stdout),
                                @"Expected:\s*([0-9a-f ]+)\s*Got:\s*([0-9a-f ]+)");
                            Check(matches.Count == 2, "both upstream AES vector outputs are present");
                            foreach (Match match in matches)
                                Check(match.Groups[1].Value.Trim() == match.Groups[2].Value.Trim(),
                                    "printed AES expected ciphertext matches generated ciphertext");
                        }
                        current.Status = "passed";
                    }
                }
                catch (Exception error) { current.Status = "failed"; current.Reason = error.Message; }
                Console.WriteLine($"{item.Name}: {program}: {current.Status}");
            }
        }
        CompareVectorTranscripts();
        bool passed = results.SelectMany(x => x.Cases).Any(x => x.Status == "passed")
            && results.SelectMany(x => x.Cases).All(x => x.Status != "failed");
        var hashes = Directory.GetFiles(Path.Combine(source, "tests")).Order()
            .ToDictionary(x => "tests/" + Path.GetFileName(x), x => Hash(File.ReadAllBytes(x)));
        hashes["utils/smb2-cp.c"] = Hash(File.ReadAllBytes(Path.Combine(source, "utils/smb2-cp.c")));
        var receipt = new
        {
            passed, complete = results.All(x => x.Cases.All(c => c.Status == "passed")),
            statusCounts = results.SelectMany(x => x.Cases).GroupBy(x => x.Status)
                .ToDictionary(x => x.Key, x => x.Count()),
            note = "Original C programs with managed orchestration of upstream shell assertions; skips are not passes. No claim that shell scripts or Valgrind ran.",
            sourceRoot = source, sourceSha256 = hashes, variants = results
        };
        await File.WriteAllTextAsync(Path.Combine(artifacts, "result.json"), JsonSerializer.Serialize(receipt, Json.Options));
        return passed ? 0 : 1;
    }

    CaseReceipt CreateCase(string name)
    {
        var receipt = new CaseReceipt(name);
        receipt.SourceSha256["tests/" + name] = Hash(File.ReadAllBytes(Path.Combine(source, "tests", name)));
        if (name.EndsWith(".sh", StringComparison.Ordinal))
        {
            receipt.SourceSha256["tests/functions.sh"] = Hash(File.ReadAllBytes(Path.Combine(source, "tests/functions.sh")));
            receipt.Adaptations.Add("C# process and file orchestration replaces the shell; original C program bodies and upstream exit/content expectations are preserved");
        }
        receipt.Adaptations.Add("Each executable invocation uses a fresh process, binary stdout/stderr capture, and an external timeout");
        if (name is "test_0300_cat_basic.sh" or "test_0310_cancel_pdu.sh")
            receipt.Adaptations.Add("Additional stdout content checks establish that upstream mains actually read the fixture despite success returns on some errors");
        if (name == "aes128ccm-test.c")
            receipt.Adaptations.Add("Additionally compare the two printed expected/encrypted vectors; upstream itself asserts only successful decryption and plaintext equality");
        if (name == "test_0600_ssc_basic.sh")
            receipt.Adaptations.Add("Deterministic BCL random fixture bytes shared across variants replace dd from /dev/urandom, preserving every file length, appended x byte, and command argument");
        return receipt;
    }
    void CompareVectorTranscripts()
    {
        var native = results.FirstOrDefault(x => x.Name == "native");
        foreach (var result in results.Where(x => x.Name != "native"))
            foreach (var receipt in result.Cases.Where(x => x.Name is "aes128ccm-test.c" or "ntlmssp_generate_blob.c"))
            {
                if (receipt.Status != "passed") continue;
                var baseline = native?.Cases.FirstOrDefault(x => x.Name == receipt.Name && x.Status == "passed");
                if (baseline == null)
                {
                    receipt.Adaptations.Add("No passing native transcript supplied; only upstream assertions and explicit vector checks were evaluated");
                    continue;
                }
                var observed = receipt.Invocations.Single();
                var expected = baseline.Invocations.Single();
                if (observed.StdoutSha256 == expected.StdoutSha256 && observed.StderrSha256 == expected.StderrSha256)
                    receipt.Checks.Add("Binary stdout and stderr exactly match the passing native vector program");
                else
                {
                    receipt.Status = "failed";
                    receipt.Reason = "Vector stdout/stderr differs from the passing native program";
                }
            }
    }

    string Url(string leaf) => (variant.TestUrl ?? manifest.TestUrl).TrimEnd('/') + "/" + leaf;
    bool Require(params string[] programs)
    {
        foreach (string program in programs)
        {
            if (!manifest.BaselineBlockedPrograms.TryGetValue(program, out var evidence)) continue;
            current.Status = variant.Name == "native" ? "baseline-failed" : "blocked";
            current.BaselineEvidence = evidence;
            current.Reason = evidence.ValueKind == JsonValueKind.Object
                && evidence.TryGetProperty("reason", out var reason) && reason.ValueKind == JsonValueKind.String
                ? reason.GetString() : $"Native baseline failed for {program}";
            current.Adaptations.Add("Invocation withheld because the unchanged upstream program failed its native baseline; original test and expectations are not modified");
            return false;
        }
        string[] missing = programs.Where(x => !variant.Programs.TryGetValue(x, out var cmd) || cmd.Length == 0).ToArray();
        if (missing.Length == 0) return true;
        Skip("Executable commands missing: " + string.Join(", ", missing));
        return false;
    }
    void Skip(string reason) { current.Status = "skipped"; current.Reason = reason; }
    void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidDataException(description);
        current.Checks.Add(description);
    }
    async Task<ProcessResult> Success(string program, params string[] arguments)
    {
        var result = await Invoke(program, arguments);
        Check(!result.TimedOut && result.ExitCode == 0, $"{program} exits successfully");
        return result;
    }
    async Task RunShellCase(string name)
    {
        switch (name)
        {
            case "test_0200_mkdir.sh":
                if (!Require("prog_mkdir", "prog_rmdir")) return;
                await Success("prog_mkdir", Url("testdir"));
                await Success("prog_rmdir", Url("testdir"));
                break;
            case "test_0210_cp_basic.sh":
                if (!Require("smb2-cp")) return;
                string input = Path.Combine(directory, "testfile");
                string output = Path.Combine(directory, "testfile2");
                await File.WriteAllBytesAsync(input, Encoding.ASCII.GetBytes("HappyPenguins!\n"));
                File.Delete(output);
                await Success("smb2-cp", input, Url("testfile"));
                await Success("smb2-cp", Url("testfile"), output);
                Check(File.ReadAllBytes(input).SequenceEqual(File.ReadAllBytes(output)), "uploaded and downloaded file contents match");
                var absent = await Invoke("smb2-cp", Url("testfile-not-exist"), output);
                Check(!absent.TimedOut && absent.ExitCode != 0, "copying a nonexistent source fails");
                break;
            case "test_0300_cat_basic.sh":
            case "test_0310_cancel_pdu.sh":
                string program = name == "test_0300_cat_basic.sh" ? "prog_cat" : "prog_cat_cancel";
                if (!Require("smb2-cp", program)) return;
                string fixture = Path.Combine(source, "tests/prog_cat.c");
                await Success("smb2-cp", fixture, Url("CAT"));
                var cat = await Success(program, Url("CAT"));
                // These extra observations prevent the upstream programs' success exits on some errors
                // from becoming false evidence that the translated read/cancellation actually ran.
                byte[] expected = File.ReadAllBytes(fixture);
                if (program == "prog_cat")
                    Check(cat.Stdout.SequenceEqual(expected), "cat stdout exactly matches the uploaded upstream source");
                else
                {
                    Check(cat.Stdout.AsSpan().IndexOf(expected) >= 0, "cancel test subsequently reads the complete source file");
                    string text = Encoding.UTF8.GetString(cat.Stdout);
                    Check(text.Contains("Cancelling PDU "), "upstream OPEN PDU cancellation was reached");
                    Check(!text.Contains("Should never reach here"), "cancelled PDU callback was not invoked");
                }
                break;
            case "test_0500_setsd_basic.sh":
                if (!Require("smb2-cp", "prog_setsd")) return;
                await Success("smb2-cp", Path.Combine(source, "tests/prog_setsd.c"), Url("SETSD"));
                await Success("prog_setsd", Url("SETSD"));
                break;
            case "test_0600_ssc_basic.sh":
                if (!Require("smb2-cp", "prog_ssc")) return;
                await ServerSideCopy();
                break;
            case "test_0100_ls_basic.sh":
                Skip("prog_ls unconditionally interposes malloc/calloc using dlsym(RTLD_NEXT); that test runtime bridge is not implemented");
                break;
            case "test_0311_open_timeout.sh":
                Skip("Requires the upstream scrambla tests/libsmb2_issue_484 server that intentionally leaves CREATE unanswered");
                break;
            case "test_0400_overdrawn_0202.sh":
                Skip("Ten-process credit stress orchestration and prog_ls allocator interposition remain unimplemented");
                break;
            case "test_900_dcerpc.sh":
                Skip("Optional upstream libdcerpc profile is not translated");
                break;
            default:
                Skip(name.Contains("socket_error") || name.Contains("malloc_error")
                    ? "Explicit upstream fault test requires native tracing/interposition and equivalent managed hooks; no replacement injection is claimed"
                    : "Native Valgrind instrumentation has no equivalent evidence in this managed runner");
                break;
        }
    }

    async Task ServerSideCopy()
    {
        string input = Path.Combine(directory, "src.bin"), output = Path.Combine(directory, "dest.local");
        int uploadSequence = 0;
        async Task Upload(int length)
        {
            byte[] bytes = new byte[length];
            new Random(0x534d4200 + uploadSequence).NextBytes(bytes);
            if (length == 1048577) bytes[^1] = (byte)'x';
            current.FixtureSha256[$"{uploadSequence:D2}-src.bin-{length}"] = Hash(bytes);
            uploadSequence++;
            await File.WriteAllBytesAsync(input, bytes);
            await Success("smb2-cp", input, Url("src.bin"));
        }
        async Task Copy(int length, string operation, string mode, string control, params string[] extra)
        {
            await Upload(length);
            await Success("prog_ssc", [operation, mode, control, Url("src.bin"), Url("dest.bin"), .. extra]);
            File.Delete(output);
            await Success("smb2-cp", Url("dest.bin"), output);
            Check(File.ReadAllBytes(input).SequenceEqual(File.ReadAllBytes(output)), $"{length} bytes: {operation} {mode} {control} content matches");
        }
        await Copy(0, "server-side-copy", "sync", "copychunk");
        await Copy(4096, "server-side-copy", "sync", "copychunk");
        await Copy(1048575, "server-side-copy", "sync", "copychunk");
        await Copy(1048576, "server-side-copy", "sync", "copychunk", "1048576", "16", "0", "1048576");
        await Copy(1048577, "server-side-copy", "sync", "copychunk_write");
        await Copy(1048576, "server-side-copy", "async", "copychunk");
        await Copy(1048576, "server-side-copy", "async", "copychunk_write");
        foreach (string mode in new[] { "sync", "async" })
            foreach (string control in new[] { "copychunk", "copychunk_write" })
                await Copy(1048576, "copychunk", mode, control);
        await Copy(20 * 1048576, "server-side-copy", "sync", "copychunk");
        await Copy(20 * 1048576, "server-side-copy", "sync", "copychunk_write");
        var invalid = await Invoke("prog_ssc", "server-side-copy", "sync", "0", Url("src.bin"), Url("dest.bin"));
        Check(!invalid.TimedOut && invalid.ExitCode != 0, "invalid ctl_code is rejected");
        await Upload(512);
        var rejected = await Invoke("prog_ssc", "server-side-copy", "sync", "copychunk_write", Url("src.bin"), Url("dest.bin"), "1", "257");
        Check(!rejected.TimedOut && rejected.ExitCode > 0 && rejected.ExitCode < 128, "server rejects excessive copychunk count without process signal");
        CheckReply(rejected);
        await Upload(65536);
        var overlap = await Invoke("prog_ssc", "server-side-copy", "sync", "copychunk_write", Url("src.bin"), Url("src.bin"));
        Check(!overlap.TimedOut && overlap.ExitCode >= 0 && overlap.ExitCode < 128, "self-overlap completes without process signal (server may accept or reject)");
        CheckReply(overlap);
        await Copy(4096, "server-side-copy", "sync", "copychunk");
        File.Delete(input);
        File.Delete(output);
    }
    void CheckReply(ProcessResult result)
    {
        string output = Encoding.UTF8.GetString(result.Stdout) + Encoding.UTF8.GetString(result.Stderr);
        Check(!output.Contains("Failed to parse") && !output.Contains("Unexpected size of Error reply"), "IOCTL status reply is not misparsed");
    }
    async Task<ProcessResult> Invoke(string program, params string[] arguments)
    {
        string sourcePath = program == "smb2-cp" ? "utils/smb2-cp.c" : "tests/" + program + ".c";
        current.SourceSha256[sourcePath] = Hash(File.ReadAllBytes(Path.Combine(source, sourcePath)));
        var command = variant.Programs[program];
        var start = new ProcessStartInfo(command[0])
        {
            WorkingDirectory = directory, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true,
            RedirectStandardInput = true
        };
        foreach (string arg in command.Skip(1).Concat(arguments)) start.ArgumentList.Add(arg);
        foreach (var item in manifest.Environment) start.Environment[item.Key] = item.Value;
        foreach (var item in variant.Environment) start.Environment[item.Key] = item.Value;
        start.Environment["TESTURL"] = variant.TestUrl ?? manifest.TestUrl;
        int sequence = current.Invocations.Count;
        string prefix = Path.Combine(directory, $"{sequence:D3}-{program}");
        string stdoutPath = prefix + ".stdout.bin", stderrPath = prefix + ".stderr.bin";
        await using var stdout = File.Create(stdoutPath);
        await using var stderr = File.Create(stderrPath);
        using var process = new Process { StartInfo = start };
        var watch = Stopwatch.StartNew();
        process.Start();
        process.StandardInput.Close();
        Task stdoutTask = process.StandardOutput.BaseStream.CopyToAsync(stdout);
        Task stderrTask = process.StandardError.BaseStream.CopyToAsync(stderr);
        bool timedOut = false;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(manifest.TimeoutSeconds));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            timedOut = true;
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            await process.WaitForExitAsync();
        }
        await Task.WhenAll(stdoutTask, stderrTask);
        await stdout.DisposeAsync();
        await stderr.DisposeAsync();
        watch.Stop();
        byte[] outBytes = await File.ReadAllBytesAsync(stdoutPath), errBytes = await File.ReadAllBytesAsync(stderrPath);
        current.Invocations.Add(new(program, process.ExitCode, timedOut, watch.ElapsedMilliseconds,
            Path.GetRelativePath(artifacts, stdoutPath), Path.GetRelativePath(artifacts, stderrPath), Hash(outBytes), Hash(errBytes)));
        return new(process.ExitCode, timedOut, outBytes, errBytes);
    }
    static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
