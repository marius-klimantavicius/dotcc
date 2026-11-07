using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation;

if (args.Length is < 3 or > 7 || !Enum.TryParse<ExecutionMode>(args[0], out var mode))
    throw new ArgumentException("ServiceProbe InProcess|SeparateProcess closure.json report.json [seconds] [budget]");
int seconds = args.Length > 3 ? int.Parse(args[3]) : 180;
long budget = args.Length > 4 ? long.Parse(args[4]) : 150_000_000;
if (seconds is < 1 or > 7200 || budget is < 1 or > 3_000_000_000L) throw new ArgumentOutOfRangeException(nameof(args));
using var closure = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var root = closure.RootElement;
string executable = root.TryGetProperty("Executable", out var entry) ? entry.GetString()! : "/dotnet/dotnet";
var environment = root.GetProperty("Environment").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString()!);
BlinkMachine? machine = null;
MachineRun? run = null;
MachineRunResult? result = null;
bool passed = false, runDisposed = false, machineDisposed = false, workerGone = false;
int? worker = null;
string? error = null, cleanupError = null;
var responses = new List<(string Path, int Status, string Body, double Ms)>();
var time = Stopwatch.StartNew(); double readyMs = 0;
try
{
    machine = new(new MachineOptions { ExecutionMode = mode, MemoryLimit = 256L << 20,
        VirtualMemoryLimit = 4L << 30, InstructionLimit = budget,
        ExecutionDeadline = TimeSpan.FromSeconds(seconds), Environment = environment,
        GuestJit = new() { Enabled = args.Length > 5 && args[5] == "guest-jit", AllowUnverifiedWindows = true,
            Mode = args.Length > 5 && args[5] == "full-jit" ? Managed.Emulation.Execution.GuestJitMode.Full : Managed.Emulation.Execution.GuestJitMode.Interpreter,
            HotBlockThreshold = args.Length > 6 ? int.Parse(args[6]) : 16 },
        Network = new NetworkPolicy { Publications = [new(8080)] } });
    machine.ImportImage(root.GetProperty("Files").EnumerateArray().Select(file =>
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(root.GetProperty("Images").GetString()!, file.GetProperty("File").GetString()!));
        if (Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != file.GetProperty("Sha256").GetString()) throw new IOException("Image changed");
        return new ImageFile(file.GetProperty("GuestPath").GetString()!, bytes, file.GetProperty("Executable").GetBoolean());
    }));
    run = await machine.StartAsync(new ExecutionOptions { Executable = executable,
        Arguments = executable == "/dotnet/dotnet" ? ["/apps/service/CoreClrService.dll", "8080"] : ["8080"],
        WorkingDirectory = "/work", AllowElfInterpreter = true,
        Readiness = new ReadinessOptions { Kind = ReadinessKind.ListeningPorts, Timeout = TimeSpan.FromSeconds(seconds) },
        Console = new ConsoleOptions { CaptureBytes = 65536, OutputLimit = 65536 } });
    worker = run.WorkerProcessId;
    var endpoint = (await run.Ready).Single(p => p.GuestPort == 8080);
    readyMs = time.Elapsed.TotalMilliseconds;
    foreach (var request in new[] { ("GET", "/health", "", "CORECLR SERVICE OK\n"),
        ("GET", "/health", "", "CORECLR SERVICE OK\n"), ("GET", "/health", "", "CORECLR SERVICE OK\n"),
        ("POST", "/echo", "payload", "CORECLR:payload\n"), ("POST", "/echo", "payload", "CORECLR:payload\n"),
        ("POST", "/stop", "", "STOP OK\n") })
    {
        var timer = Stopwatch.StartNew();
        using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(seconds) };
        using var message = new HttpRequestMessage(new HttpMethod(request.Item1), $"http://{endpoint.HostAddress}:{endpoint.HostPort}{request.Item2}")
            { Content = new StringContent(request.Item3) };
        using var response = await client.SendAsync(message);
        string body = await response.Content.ReadAsStringAsync();
        responses.Add((request.Item2, (int)response.StatusCode, body, timer.Elapsed.TotalMilliseconds));
        if (response.StatusCode != HttpStatusCode.OK || body != request.Item4) throw new IOException("HTTP response differs");
    }
    result = await run.WaitAsync();
    passed = result.Reason == RunExitReason.Exited && result.ExitCode == 0 && result.ResourcesReleased &&
        !result.CaptureTruncated && result.StandardError.Length == 0 &&
        Encoding.UTF8.GetString(result.StandardOutput) == "SERVICE READY\nSERVICE STOPPED\n";
    if (args.Length > 5 && args[5] == "full-jit")
        passed &= result.JitStatistics is { Mode: "Full", InterpreterInstructions: 0, InterpreterAttempts: 0 } strict &&
            strict.InlineNativeInstructions + strict.SemanticHelperInstructions == result.Instructions;
}
catch (Exception failure) { error = failure.ToString(); }
finally
{
    if (run != null)
        try
        {
            if (!run.Completion.IsCompleted) await run.StopAsync();
            result ??= await run.WaitAsync();
            await run.DisposeAsync(); runDisposed = true;
        }
        catch (Exception failure) { cleanupError = failure.ToString(); }
    if (machine != null)
        try { await machine.DisposeAsync(); machineDisposed = true; }
        catch (Exception failure) { cleanupError = (cleanupError ?? "") + failure; }
    if (worker is int pid)
        try { using var process = Process.GetProcessById(pid); workerGone = process.HasExited; }
        catch (ArgumentException) { workerGone = true; }
    else workerGone = true;
    passed &= error == null && cleanupError == null && runDisposed && machineDisposed && workerGone &&
        (mode == ExecutionMode.SeparateProcess ? worker is > 0 : worker == null);
    using var file = File.Create(args[2]); using var json = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true });
    json.WriteStartObject(); json.WriteBoolean("passed", passed);
    json.WriteString("mode", mode.ToString()); json.WriteBoolean("host_dynamic_code", RuntimeFeature.IsDynamicCodeCompiled);
    json.WriteNumber("ready_ms", readyMs); json.WriteNumber("total_ms", time.Elapsed.TotalMilliseconds);
    json.WriteNumber("instructions", result?.Instructions ?? 0); json.WriteBoolean("resources_released", result?.ResourcesReleased == true);
    json.WriteStartObject("jit");
    json.WriteString("mode", result?.JitStatistics?.Mode);
    json.WriteNumber("inline_native_instructions", result?.JitStatistics?.InlineNativeInstructions ?? 0);
    json.WriteNumber("semantic_helper_instructions", result?.JitStatistics?.SemanticHelperInstructions ?? 0);
    json.WriteNumber("semantic_helper_calls", result?.JitStatistics?.SemanticHelperCalls ?? 0);
    json.WriteNumber("interpreter_attempts", result?.JitStatistics?.InterpreterAttempts ?? 0);
    json.WriteNumber("native_tail_jumps", result?.JitStatistics?.NativeTailJumps ?? 0);
    json.WriteNumber("translation_misses", result?.JitStatistics?.TranslationMisses ?? 0);
    json.WriteNumber("code_validation_misses", result?.JitStatistics?.CodeValidationMisses ?? 0);
    json.WriteNumber("semantic_plan_bindings", result?.JitStatistics?.SemanticPlanBindings ?? 0);
    json.WriteNumber("semantic_cache_hits", result?.JitStatistics?.SemanticCacheHits ?? 0);
    json.WriteNumber("semantic_capacity_evictions", result?.JitStatistics?.SemanticCapacityEvictions ?? 0);
    json.WriteNumber("semantic_cache_capacity", result?.JitStatistics?.SemanticCacheCapacity ?? 0);
    json.WriteNumber("semantic_cache_entries_after_release", result?.JitStatistics?.SemanticCacheEntries ?? 0);
    json.WriteNumber("semantic_peak_cache_entries", result?.JitStatistics?.SemanticPeakCacheEntries ?? 0);
    json.WriteNumber("semantic_directory_probes", result?.JitStatistics?.SemanticDirectoryProbes ?? 0);
    json.WriteNumber("semantic_directory_collisions", result?.JitStatistics?.SemanticDirectoryCollisions ?? 0);
    json.WriteNumber("compile_allocated_bytes", result?.JitStatistics?.CompileAllocatedBytes ?? 0);
    json.WriteNumber("emitted_code_bytes", result?.JitStatistics?.EmittedCodeBytes ?? 0);
    json.WriteString("capability", result?.JitStatistics?.Capability);
    json.WriteNumber("compiled_blocks", result?.JitStatistics?.CompiledBlocks ?? 0);
    // Retained TB counters are separate from the historical semantic-entry
    // fields above. Current residency is zero after release; peaks and totals
    // describe the completed owner, not a lifetime count of distinct keys.
    json.WriteNumber("unique_block_keys_current", result?.JitStatistics?.UniqueBlockKeys ?? 0);
    json.WriteNumber("retained_block_executions", result?.JitStatistics?.RetainedBlockExecutions ?? 0);
    json.WriteNumber("lookup_tier_misses", result?.JitStatistics?.LookupTierMisses ?? 0);
    json.WriteNumber("worker_key_lookups", result?.JitStatistics?.WorkerKeyLookups ?? 0);
    json.WriteNumber("shared_index_lookups", result?.JitStatistics?.SharedIndexLookups ?? 0);
    json.WriteNumber("resume_token_lookups", result?.JitStatistics?.ResumeTokenLookups ?? 0);
    json.WriteNumber("link_target_lookups", result?.JitStatistics?.LinkTargetLookups ?? 0);
    json.WriteNumber("link_reuse_hits", result?.JitStatistics?.LinkReuseHits ?? 0);
    json.WriteNumber("deferred_link_preparations", result?.JitStatistics?.DeferredLinkPreparations ?? 0);
    json.WriteNumber("compile_attempts", result?.JitStatistics?.CompileAttempts ?? 0);
    json.WriteNumber("actual_block_compilations", result?.JitStatistics?.ActualBlockCompilations ?? 0);
    json.WriteNumber("capacity_evictions", result?.JitStatistics?.BudgetEvictions ?? 0);
    json.WriteNumber("content_invalidations", result?.JitStatistics?.ContentInvalidations ?? 0);
    json.WriteNumber("authority_revalidations", result?.JitStatistics?.AuthorityRevalidations ?? 0);
    json.WriteNumber("resident_translation_blocks_after_release", result?.JitStatistics?.ResidentTranslationBlocks ?? 0);
    json.WriteNumber("peak_resident_translation_blocks", result?.JitStatistics?.PeakResidentTranslationBlocks ?? 0);
    json.WriteNumber("retained_native_bytes_after_release", result?.JitStatistics?.RetainedNativeBytes ?? 0);
    json.WriteNumber("shared_scaffold_emitted_bytes", result?.JitStatistics?.SharedScaffoldEmittedBytes ?? 0);
    json.WriteNumber("shared_scaffold_reserved_bytes_after_release", result?.JitStatistics?.SharedScaffoldReservedBytes ?? 0);
    json.WriteNumber("peak_shared_scaffold_reserved_bytes", result?.JitStatistics?.PeakSharedScaffoldReservedBytes ?? 0);
    // Packed arena occupancy and cumulative flush/protection diagnostics.
    json.WriteNumber("useful_published_code_bytes", result?.JitStatistics?.UsefulPublishedCodeBytes ?? 0);
    json.WriteNumber("peak_useful_published_code_bytes", result?.JitStatistics?.PeakUsefulPublishedCodeBytes ?? 0);
    json.WriteNumber("aligned_occupied_code_bytes", result?.JitStatistics?.AlignedOccupiedCodeBytes ?? 0);
    json.WriteNumber("peak_aligned_occupied_code_bytes", result?.JitStatistics?.PeakAlignedOccupiedCodeBytes ?? 0);
    json.WriteNumber("payload_used_bytes", result?.JitStatistics?.PayloadUsedBytes ?? 0);
    json.WriteNumber("peak_payload_used_bytes", result?.JitStatistics?.PeakPayloadUsedBytes ?? 0);
    json.WriteNumber("descriptor_used_bytes", result?.JitStatistics?.DescriptorUsedBytes ?? 0);
    json.WriteNumber("peak_descriptor_used_bytes", result?.JitStatistics?.PeakDescriptorUsedBytes ?? 0);
    json.WriteNumber("index_used_bytes", result?.JitStatistics?.IndexUsedBytes ?? 0);
    json.WriteNumber("peak_index_used_bytes", result?.JitStatistics?.PeakIndexUsedBytes ?? 0);
    json.WriteNumber("fixed_charged_bytes", result?.JitStatistics?.FixedChargedBytes ?? 0);
    json.WriteNumber("peak_fixed_charged_bytes", result?.JitStatistics?.PeakFixedChargedBytes ?? 0);
    json.WriteNumber("arena_reserved_bytes", result?.JitStatistics?.ArenaReservedBytes ?? 0);
    json.WriteNumber("peak_arena_reserved_bytes", result?.JitStatistics?.PeakArenaReservedBytes ?? 0);
    json.WriteNumber("arena_used_bytes", result?.JitStatistics?.ArenaUsedBytes ?? 0);
    json.WriteNumber("data_arena_used_bytes", result?.JitStatistics?.DataArenaUsedBytes ?? 0);
    json.WriteNumber("data_arena_reserved_bytes", result?.JitStatistics?.DataArenaReservedBytes ?? 0);
    json.WriteNumber("data_arena_slack_bytes", result?.JitStatistics?.DataArenaSlackBytes ?? 0);
    json.WriteNumber("data_arena_overhead_bytes", result?.JitStatistics?.DataArenaOverheadBytes ?? 0);
    json.WriteNumber("code_reserved_bytes", result?.JitStatistics?.CodeReservedBytes ?? 0);
    json.WriteNumber("region_slack_bytes", result?.JitStatistics?.RegionSlackBytes ?? 0);
    json.WriteNumber("page_slack_bytes", result?.JitStatistics?.PageSlackBytes ?? 0);
    json.WriteNumber("protection_calls", result?.JitStatistics?.ProtectionCalls ?? 0);
    json.WriteNumber("total_flushes", result?.JitStatistics?.TotalFlushes ?? 0);
    json.WriteNumber("flush_retirements", result?.JitStatistics?.FlushRetirements ?? 0);
    json.WriteNumber("flush_requests", result?.JitStatistics?.FlushRequests ?? 0);
    json.WriteNumber("flush_denials", result?.JitStatistics?.FlushDenials ?? 0);
    json.WriteNumber("setup_allocated_bytes", result?.JitStatistics?.SetupAllocatedBytes ?? 0);
    json.WriteNumber("helper_managed_allocated_bytes", result?.JitStatistics?.HelperManagedAllocatedBytes ?? 0);
    json.WriteNumber("publication_allocated_bytes", result?.JitStatistics?.PublicationAllocatedBytes ?? 0);
    json.WriteNumber("diagnostics_allocated_bytes", result?.JitStatistics?.DiagnosticsAllocatedBytes ?? 0);
    json.WriteNumber("physical_authority_pages", result?.JitStatistics?.PhysicalAuthorityPages ?? 0);
    json.WriteBoolean("authority_registry_saturated", result?.JitStatistics?.AuthorityRegistrySaturated ?? false);
    json.WriteNumber("host_backing_mutation_scopes", result?.JitStatistics?.HostBackingMutationScopes ?? 0);
    json.WriteNumber("native_authority_enrollment_misses", result?.JitStatistics?.NativeAuthorityEnrollmentMisses ?? 0);
    json.WriteNumber("native_entries", result?.JitStatistics?.NativeEntries ?? 0);
    json.WriteNumber("first_child_instructions", result?.JitStatistics?.FirstChildInstructions ?? -1);
    json.WriteNumber("native_at_first_child", result?.JitStatistics?.NativeInstructionsAtFirstChild ?? 0);
    json.WriteNumber("native_after_first_child", result?.JitStatistics is { FirstChildInstructions: >= 0 } stats
        ? stats.GuestInstructions - stats.NativeInstructionsAtFirstChild : 0);
    json.WriteNumber("guest_instructions", result?.JitStatistics?.GuestInstructions ?? 0);
    json.WriteNumber("interpreted_instructions", result?.JitStatistics?.InterpreterInstructions ?? 0);
    json.WriteNumber("peak_cache_bytes", result?.JitStatistics?.PeakCacheBytes ?? 0);
    json.WriteNumber("cache_bytes_after_release", result?.JitStatistics?.CacheBytes ?? 0);
    json.WriteEndObject();
    json.WriteString("reason", result?.Reason.ToString()); json.WriteString("stdout", result == null ? null : Encoding.UTF8.GetString(result.StandardOutput));
    json.WriteString("stderr", result == null ? null : Encoding.UTF8.GetString(result.StandardError));
    if (worker is int id) json.WriteNumber("worker", id); else json.WriteNull("worker");
    json.WriteBoolean("worker_gone", workerGone); json.WriteBoolean("run_disposed", runDisposed); json.WriteBoolean("machine_disposed", machineDisposed);
    json.WriteString("error", error); json.WriteString("cleanup_error", cleanupError);
    json.WriteStartArray("responses");
    foreach (var response in responses)
    {
        json.WriteStartObject(); json.WriteString("path", response.Path); json.WriteNumber("status", response.Status);
        json.WriteString("body", response.Body); json.WriteNumber("ms", response.Ms); json.WriteEndObject();
    }
    json.WriteEndArray(); json.WriteEndObject();
}
return passed ? 0 : 1;
