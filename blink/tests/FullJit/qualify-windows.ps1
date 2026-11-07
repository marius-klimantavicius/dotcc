# Windows HOST qualification only. All inputs are frozen Linux-produced assets.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$OrdinaryFixtures,
    [Parameter(Mandatory)][string]$CpuFixtures,
    [Parameter(Mandatory)][string]$SignalFixtures,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$SourceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Before creating output, compiling the observer, looking for dotnet, or running guests.
if ($PSVersionTable.PSVersion.Major -lt 7 -or
    -not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or
    -not $IsWindows -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne 'X64' -or
    [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne 'X64') {
    throw 'UNVERIFIED_HOST: real Windows x64 OS and x64 PowerShell 7 are required. Linux/pwsh parsing and win-x64 cross-builds cannot produce a Windows PASS.'
}

function Fail([string]$Status, [string]$Message) {
    $failure = [InvalidOperationException]::new($Message)
    $failure.Data['QualificationStatus'] = $Status
    throw $failure
}
function Write-Json([string]$Path, $Value) {
    ConvertTo-Json -InputObject $Value -Depth 40 | Set-Content -LiteralPath $Path -Encoding utf8
}
function Resolve-Directory([string]$Path) {
    $item = Get-Item -LiteralPath $Path
    if (-not $item.PSIsContainer) { throw "Expected a directory: $Path" }
    return $item.FullName
}
function Test-Within([string]$Path, [string]$Directory) {
    $prefix = $Directory.TrimEnd([char[]]@('\', '/')) + [IO.Path]::DirectorySeparatorChar
    return $Path.Equals($Directory, [StringComparison]::OrdinalIgnoreCase) -or
        $Path.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)
}
function Get-Hash([string]$Path) { return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Assert-Elf([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    try {
        $header = [byte[]]::new(64)
        if ($stream.Read($header, 0, 64) -ne 64 -or
            [BitConverter]::ToString($header[0..5]) -ne '7F-45-4C-46-02-01' -or
            [BitConverter]::ToUInt16($header, 18) -ne 62) {
            throw "Expected a Linux-produced ELF64 little-endian x86-64 guest: $Path"
        }
    } finally { $stream.Dispose() }
}
function Assert-WindowsPe([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $reader = [IO.BinaryReader]::new($stream)
    try {
        if ($reader.ReadUInt16() -ne 0x5a4d) { throw "Missing Windows PE executable: $Path" }
        $stream.Position = 0x3c
        $offset = $reader.ReadUInt32()
        $stream.Position = $offset
        if ($reader.ReadUInt32() -ne 0x4550 -or $reader.ReadUInt16() -ne 0x8664) {
            throw "Expected an AMD64 Windows HOST executable: $Path"
        }
    } finally { $reader.Dispose() }
}

$source = Resolve-Directory $SourceRoot
$ordinary = Resolve-Directory $OrdinaryFixtures
$cpu = Resolve-Directory $CpuFixtures
# Accept the distinct signal directory, or its manifest.json for convenient transfer.
$signalItem = Get-Item -LiteralPath $SignalFixtures
if (-not $signalItem.PSIsContainer -and $signalItem.Name -ne 'manifest.json') { throw 'SignalFixtures must name its directory or manifest.json.' }
$signals = if ($signalItem.PSIsContainer) { $signalItem.FullName } else { $signalItem.DirectoryName }
$out = [IO.Path]::GetFullPath($OutputDirectory)
foreach ($inputRoot in @($source, $ordinary, $cpu, $signals)) {
    if ((Test-Within $out $inputRoot) -or (Test-Within $inputRoot $out)) {
        throw 'OutputDirectory must be outside, and cannot contain, the source and fixture trees.'
    }
}
if ($source -match '[;,]' -or $out -match '[;,]') { throw 'MSBuild property paths cannot contain semicolons or commas.' }
if (Test-Path -LiteralPath $out) {
    if (@(Get-ChildItem -LiteralPath $out -Force).Count -ne 0) { throw 'Use a fresh, empty OutputDirectory; stale results cannot qualify a host.' }
}
[void](New-Item -ItemType Directory -Path $out -Force)
foreach ($directory in @('tmp', 'logs', 'reports', 'publish', 'build', 'nuget/packages', 'nuget/http', 'nuget/plugins', 'nuget/scratch', 'dotnet-home', 'powershell')) {
    [void](New-Item -ItemType Directory -Path (Join-Path $out $directory) -Force)
}
$commands = [Collections.Generic.List[object]]::new()
$cells = [Collections.Generic.List[object]]::new()
$binaryCache = @{}
$environmentBackup = @{}
$sourceBefore = $null
$fixturesBefore = $null
$matrixComplete = $false
$summary = [ordered]@{
    status = 'RUNNING'; windows_pass = $false; actual_windows_host = $true; guests = 'Linux x86-64 ELF'
    started_utc = [DateTime]::UtcNow.ToString('o'); source_root = $source; output_directory = $out
    runner_sha256 = Get-Hash $PSCommandPath; source_stable = $false; fixtures_stable = $false
    qualification_scope = 'Frozen snapshot; processed/raw x CoreCLR/NativeAOT; existing supported mitigation profile only'
    source_production = 'Not performed or certified by this runner'; cells = $cells; error = $null
}
function Save-Receipts {
    Write-Json (Join-Path $out 'commands.json') @($commands.ToArray())
    Write-Json (Join-Path $out 'qualification.json') $summary
}
function Get-SourceInventory {
    $files = [Collections.Generic.List[string]]::new()
    foreach ($relative in @('blink/src', 'blink/generated/TranslatedBlink', 'blink/generated/TranslatedBlink.Raw',
        'blink/tests/FullJit', 'blink/tests/FullJitSignals', 'blink/tests/GuestJit/ContractTests')) {
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $source $relative) -Recurse -File -Force) {
            $name = [IO.Path]::GetRelativePath($source, $file.FullName).Replace('\', '/')
            if ($name -notmatch '(^|/)(bin|obj|\.git|__pycache__)(/|$)') { $files.Add($file.FullName) }
        }
    }
    # Include ancestor SDK/NuGet/MSBuild settings, including optional local overrides.
    foreach ($relative in @('', 'blink', 'blink/src', 'blink/generated', 'blink/tests', 'blink/tests/GuestJit')) {
        foreach ($name in @('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'global.json', 'NuGet.Config', 'nuget.config')) {
            $path = Join-Path (Join-Path $source $relative) $name
            if (Test-Path -LiteralPath $path -PathType Leaf) { $files.Add($path) }
        }
    }
    foreach ($relative in @('blink/artifacts/jit-implementation/full-jit-20261027-r1/review-native-r1.md',
        'blink/artifacts/jit-implementation/full-jit-20261027-r1/review-native-r1.json',
        'blink/config/source-manifest.json', 'blink/artifacts/campaign/current-threaded.json')) {
        $path = Join-Path $source $relative
        if (Test-Path -LiteralPath $path -PathType Leaf) { $files.Add($path) }
    }
    return @(foreach ($file in ($files | Sort-Object -Unique)) {
        [ordered]@{ path = [IO.Path]::GetRelativePath($source, $file).Replace('\', '/'); sha256 = Get-Hash $file }
    })
}
function Get-FixtureInventory {
    # The CPU directory is a transfer unit, including optional native captures,
    # pins, sources and nested files. Rediscover it after execution to detect additions/deletions.
    $paths = @($fixturePaths) + @(Get-ChildItem -LiteralPath $cpu -Recurse -File -Force | ForEach-Object { $_.FullName })
    return @(foreach ($file in ($paths | Sort-Object -Unique)) {
        [ordered]@{ path = $file; sha256 = Get-Hash $file }
    })
}
function Get-CpuInputs([string]$Directory) {
    foreach ($required in @('corpus.json', 'cpu-0.elf')) {
        if (-not (Test-Path -LiteralPath (Join-Path $Directory $required) -PathType Leaf)) { throw "Missing CPU fixture input: $required" }
    }
    $cases = @((Get-Content -LiteralPath (Join-Path $Directory 'corpus.json') -Raw | ConvertFrom-Json -AsHashtable).cases | Where-Object { $_.fault -eq 0 })
    if ($cases.Count -eq 0) { throw 'CPU corpus contains no nonfault cases.' }
    $nativeCaptures = 0
    foreach ($case in $cases) {
        $index = [int]$case.index
        $elf = Join-Path $Directory "cpu-$index.elf"
        if (-not (Test-Path -LiteralPath $elf -PathType Leaf)) { throw "Missing per-case Linux CPU guest: $elf" }
        Assert-Elf $elf
        if (Test-Path -LiteralPath (Join-Path $Directory "cpu-$index.native") -PathType Leaf) { ++$nativeCaptures }
    }
    $cpuPinsPath = Join-Path $Directory 'cpu-fixtures.json'
    $pinsSha256 = $null
    if (Test-Path -LiteralPath $cpuPinsPath -PathType Leaf) {
        $pins = Get-Content -LiteralPath $cpuPinsPath -Raw | ConvertFrom-Json -AsHashtable
        $expectedPinKeys = @($cases | ForEach-Object { "$([int]$_.index)|$($_.name)" } | Sort-Object)
        $pinKeys = @($pins.rows | ForEach-Object { "$([int]$_.index)|$($_.name)" } | Sort-Object)
        if ($pinKeys.Count -ne $expectedPinKeys.Count -or ($pinKeys -join "`n") -cne ($expectedPinKeys -join "`n")) {
            throw 'cpu-fixtures.json pins do not describe the nonfault corpus selection.'
        }
        foreach ($pin in $pins.rows) {
            $index = [int]$pin.index
            foreach ($kind in @(@('elf', 'elf_sha256'), @('native', 'native_sha256'), @('S', 'source_sha256'))) {
                $path = Join-Path $Directory "cpu-$index.$($kind[0])"
                if ((Test-Path -LiteralPath $path -PathType Leaf) -and (Get-Hash $path) -cne $pin[$kind[1]]) {
                    throw "CPU fixture pin mismatch: $path"
                }
            }
        }
        $pinsSha256 = Get-Hash $cpuPinsPath
    }
    return @{ cases = $cases; native_captures = $nativeCaptures; pins_sha256 = $pinsSha256 }
}
function Get-BinaryIdentity([string]$Path) {
    if (-not $binaryCache.ContainsKey($Path)) {
        $item = Get-Item -LiteralPath $Path
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($Path)
        $binaryCache[$Path] = [ordered]@{ path = $item.FullName; bytes = $item.Length; sha256 = Get-Hash $Path
            file_version = $version.FileVersion; product_version = $version.ProductVersion }
    }
    return $binaryCache[$Path]
}
function Get-ProcessEvidence([Diagnostics.Process]$Process, [string]$Role) {
    if ($Process.HasExited) { throw 'Process exited before its policy observation.' }
    $policies = @(foreach ($policy in @(@('dynamic_code', 2), @('cfg_xfg', 7), @('user_shadow_stack', 15))) {
        [uint32]$flags = 0; [int]$errorCode = 0
        $ok = [FullJitWindowsPolicyObserver]::Query($Process.Id, $policy[1], [ref]$flags, [ref]$errorCode)
        [ordered]@{ name = $policy[0]; id = $policy[1]; queried = $ok; flags = $flags
            flags_hex = ('0x{0:x8}' -f $flags); win32_error = $errorCode }
    })
    return [ordered]@{ role = $Role; pid = $Process.Id; started_utc = $Process.StartTime.ToUniversalTime().ToString('o')
        binary = Get-BinaryIdentity $Process.MainModule.FileName; policies = $policies }
}
function Test-PolicyUnavailable($Evidence) {
    foreach ($policy in $Evidence.policies) {
        if (-not $policy.queried) { return $true }
        if ($policy.id -eq 2 -and ($policy.flags -band 1) -ne 0) { return $true }
        if ($policy.id -in @(7, 15) -and $policy.flags -ne 0) { return $true }
    }
    return $false
}
function Find-OwnedWorkers([string]$Executable) {
    if (-not $Executable) { return }
    foreach ($candidate in [Diagnostics.Process]::GetProcessesByName('Managed.Emulation.Worker')) {
        try {
            if (-not $candidate.HasExited -and $candidate.MainModule.FileName.Equals($Executable, [StringComparison]::OrdinalIgnoreCase)) {
                $candidate # Callers own and dispose these handles.
            } else { $candidate.Dispose() }
        } catch { $candidate.Dispose() } # Transient exited processes are expected.
    }
}

function Invoke-Recorded {
    param([string]$Label, [string[]]$Argv, [string]$WorkingDirectory = $out,
        [string]$ExpectedMarker = '', [string]$WorkerExecutable = '', [switch]$RequireWorker,
        [int]$TimeoutSeconds = 2700)
    $record = [ordered]@{ label = $Label; argv = $Argv; cwd = $WorkingDirectory; status = 'RUNNING'
        started_utc = [DateTime]::UtcNow.ToString('o'); exit_code = $null; processes = [Collections.Generic.List[object]]::new()
        cleanup = 'PENDING'; forced_cleanup = $false; transient_worker_samples = 0; error = $null }
    $commands.Add($record); Save-Receipts
    $stdoutPath = Join-Path $out "logs/$Label.stdout"
    $stderrPath = Join-Path $out "logs/$Label.stderr"
    $stdout = [IO.File]::Create($stdoutPath); $stderr = [IO.File]::Create($stderrPath)
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = [Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = $Argv[0]
    for ($index = 1; $index -lt $Argv.Count; ++$index) { $process.StartInfo.ArgumentList.Add($Argv[$index]) }
    $process.StartInfo.WorkingDirectory = $WorkingDirectory
    $process.StartInfo.UseShellExecute = $false; $process.StartInfo.CreateNoWindow = $true
    $process.StartInfo.RedirectStandardOutput = $true; $process.StartInfo.RedirectStandardError = $true
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $observedWorkers = @{}; $policyUnavailable = $false; $observationUnavailable = $false
    $stdoutTask = $null; $stderrTask = $null; $started = $false
    try {
        $started = $process.Start()
        if (-not $started) { throw "Could not start $Label" }
        $stdoutTask = $process.StandardOutput.BaseStream.CopyToAsync($stdout)
        $stderrTask = $process.StandardError.BaseStream.CopyToAsync($stderr)
        try {
            $evidence = Get-ProcessEvidence $process 'controller'
            $record.processes.Add($evidence)
            $policyUnavailable = Test-PolicyUnavailable $evidence
        } catch {
            $record.processes.Add(@{ role = 'controller'; pid = $process.Id; observation_error = $_.Exception.Message })
            $observationUnavailable = $true
        }
        while ($true) {
            foreach ($worker in @(Find-OwnedWorkers $WorkerExecutable)) {
                try {
                    $key = "$($worker.Id)/$($worker.StartTime.ToUniversalTime().Ticks)"
                    if (-not $observedWorkers.ContainsKey($key)) {
                        $evidence = Get-ProcessEvidence $worker 'matching-worker'
                        if (@($evidence.policies | Where-Object { -not $_.queried }).Count -gt 0 -and $worker.HasExited) {
                            ++$record.transient_worker_samples
                            continue
                        }
                        $record.processes.Add($evidence); $observedWorkers[$key] = $true
                        if (Test-PolicyUnavailable $evidence) { $policyUnavailable = $true }
                    }
                } catch {
                    if ($worker.HasExited) { ++$record.transient_worker_samples }
                    else { $observationUnavailable = $true }
                }
                finally { $worker.Dispose() }
            }
            if ($process.WaitForExit(25)) { break }
            if ($watch.Elapsed.TotalSeconds -gt $TimeoutSeconds) { Fail 'TIMED_OUT' "$Label exceeded $TimeoutSeconds seconds." }
        }
        $record.exit_code = $process.ExitCode
        [void]$stdoutTask.GetAwaiter().GetResult(); [void]$stderrTask.GetAwaiter().GetResult()
        $stdout.Dispose(); $stderr.Dispose()
        $text = [IO.File]::ReadAllText($stdoutPath) + "`n" + [IO.File]::ReadAllText($stderrPath)
        if ($ExpectedMarker) {
            if ($policyUnavailable -or $text -match 'Windows prohibits dynamic code|Windows (CFG/XFG|hardware stack protection) profile is not qualified|Cannot query Windows .* policy|Native capability query unavailable') {
                Fail 'DENIED_OR_UNAVAILABLE_POLICY' "${Label}: native Windows policy is denied or unavailable; policy has not been changed. See logs and process evidence."
            }
            if ($text -match 'Windows x64 native backend is unverified; explicit opt-in required') {
                Fail 'UNAVAILABLE_WINDOWS_OPT_IN' "$Label lacks the consumer's qualification-only Windows opt-in; the runner cannot bypass it."
            }
            if ($text -match '(?im)^\s*SKIP\b') { Fail 'SKIPPED' "$Label reported SKIP; zero exit status cannot qualify Windows." }
            if ($observationUnavailable -or ($RequireWorker -and $observedWorkers.Count -eq 0)) {
                Fail 'UNAVAILABLE_PROCESS_EVIDENCE' "$Label lacks complete controller/matching-worker policy observations."
            }
        }
        if ($record.exit_code -ne 0) { Fail 'FAILED' "$Label exited $($record.exit_code); inspect $stdoutPath and $stderrPath." }
        if ($ExpectedMarker -and $text -notmatch $ExpectedMarker) { Fail 'INCOMPLETE' "$Label did not emit its required positive completion marker." }
        $record.status = 'PROCESS_COMPLETED'
    } catch {
        $record.status = if ($_.Exception.Data.Contains('QualificationStatus')) { $_.Exception.Data['QualificationStatus'] } else { 'FAILED' }
        $record.error = $_.Exception.Message
        throw
    } finally {
        if ($started -and -not $process.HasExited) {
            $record.forced_cleanup = $true
            $process.Kill($true); $process.WaitForExit()
        }
        # Only a worker executable in this unique publish directory is ours.
        foreach ($worker in @(Find-OwnedWorkers $WorkerExecutable)) {
            try {
                if (-not $worker.WaitForExit(3000)) {
                    $record.forced_cleanup = $true; $worker.Kill($true); $worker.WaitForExit()
                }
            } finally { $worker.Dispose() }
        }
        if ($null -ne $stdoutTask) { [void]$stdoutTask.GetAwaiter().GetResult() }
        if ($null -ne $stderrTask) { [void]$stderrTask.GetAwaiter().GetResult() }
        $stdout.Dispose(); $stderr.Dispose(); $process.Dispose()
        $record.cleanup = if ($record.forced_cleanup) { 'FORCED_TERMINATION_NONPASS' } else { 'REAPED' }
        $record.wall_seconds = $watch.Elapsed.TotalSeconds
        $record.stdout_sha256 = Get-Hash $stdoutPath; $record.stderr_sha256 = Get-Hash $stderrPath
        Save-Receipts
    }
    if ($record.forced_cleanup) {
        $record.status = 'CLEANUP_FAILED'; $record.error = "$Label required forced process cleanup; this is not a PASS."
        Save-Receipts
        Fail 'CLEANUP_FAILED' $record.error
    }
    return $record
}

function Assert-Report([string]$Path, [string[]]$ExpectedKeys, [string]$Suite) {
    $rows = @(Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable)
    $keys = @(foreach ($row in $rows) {
        if ($Suite -eq 'state') { "$($row.Label)|$($row.Mode)" }
        elseif ($Suite -eq 'signals') { "$($row.Fixture)|$($row.Execution)|$($row.Mode)" }
        else { "$($row.Label)|$($row.Execution)|$($row.Mode)" }
    })
    if ($keys.Count -ne $ExpectedKeys.Count -or
        (($keys | Sort-Object) -join "`n") -cne (($ExpectedKeys | Sort-Object) -join "`n")) {
        Fail 'INCOMPLETE_REPORT' "$Suite has missing, duplicate or unexpected observations: $Path"
    }
    foreach ($row in $rows) {
        if ($row.WorkerGone -ne $true) { Fail 'CLEANUP_FAILED' "$Suite did not reap a worker: $Path" }
        if ($Suite -eq 'state') { continue } # The executable asserts full architectural boundary parity.
        $result = if ($Suite -eq 'signals') { $row } else { $row.Result }
        $released = if ($Suite -eq 'signals') { $row.Released } else { $result.ResourcesReleased }
        $stats = if ($Suite -eq 'signals') { $row.Stats } else { $result.JitStatistics }
        if ($released -ne $true) { Fail 'CLEANUP_FAILED' "$Suite did not release resources: $Path" }
        if ($Suite -eq 'signals' -and ($row.OS -notmatch 'Windows' -or $row.Architecture -ne 'X64' -or $row.SimulatedCapabilities -ne $false)) {
            Fail 'UNVERIFIED_HOST' "Signals observation is not an actual Windows x64 host observation: $Path"
        }
        if ($row.Mode -eq 'Full') {
            if ($null -eq $stats -or $stats.Mode -ne 'Full' -or $stats.InterpreterInstructions -ne 0 -or
                $stats.InterpreterAttempts -ne 0 -or $stats.CacheBytes -ne 0 -or
                ($stats.InlineNativeInstructions + $stats.SemanticHelperInstructions) -le 0) {
                Fail 'FAILED' "$Suite lacks strict positive Full-JIT accounting: $Path"
            }
            if ($Suite -eq 'public' -and $row.Label -eq 'native-chain' -and $stats.NativeTailJumps -le 0) {
                Fail 'FAILED' "Missing positive native tail-jump witness: $Path"
            }
        }
    }
    return $rows.Count
}

try {
    $settings = @{
        TMP = (Join-Path $out 'tmp'); TEMP = (Join-Path $out 'tmp'); TMPDIR = (Join-Path $out 'tmp')
        DOTNET_CLI_HOME = (Join-Path $out 'dotnet-home'); NUGET_PACKAGES = (Join-Path $out 'nuget/packages')
        NUGET_HTTP_CACHE_PATH = (Join-Path $out 'nuget/http'); NUGET_PLUGINS_CACHE_PATH = (Join-Path $out 'nuget/plugins')
        NUGET_SCRATCH = (Join-Path $out 'nuget/scratch')
        PSModuleAnalysisCachePath = (Join-Path $out 'powershell/ModuleAnalysisCache')
        DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = 'true'; DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'; MSBUILDDISABLENODEREUSE = '1'
    }
    foreach ($name in $settings.Keys) {
        $environmentBackup[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
        [Environment]::SetEnvironmentVariable($name, $settings[$name], 'Process')
    }
    Write-Json (Join-Path $out 'task-paths.json') $settings
    # Read-only observer: no SetProcessMitigationPolicy, thread opt-out, or fault recovery.
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class FullJitWindowsPolicyObserver {
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetProcessMitigationPolicy(IntPtr process, int policy, out uint flags, UIntPtr size);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);
    public static bool Query(int pid, int policy, out uint flags, out int error) {
        flags = 0; error = 0;
        IntPtr process = OpenProcess(0x0400, false, pid);
        if (process == IntPtr.Zero) { error = Marshal.GetLastWin32Error(); return false; }
        try {
            bool ok = GetProcessMitigationPolicy(process, policy, out flags, (UIntPtr)4);
            if (!ok) error = Marshal.GetLastWin32Error();
            return ok;
        } catch (EntryPointNotFoundException) { error = 127; return false; }
        finally { CloseHandle(process); }
    }
}
'@
    $self = [Diagnostics.Process]::GetCurrentProcess()
    try { $shellEvidence = Get-ProcessEvidence $self 'powershell-observer-informational' } finally { $self.Dispose() }
    $hostInfo = [ordered]@{ os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        os_version = [Environment]::OSVersion.VersionString; os_architecture = 'X64'; process_architecture = 'X64'
        powershell = $PSVersionTable.PSVersion.ToString(); shell = $shellEvidence
        policy_scope = 'Actual effective process policies; system defaults are not inferred'
        policy_changed = $false; simulated_capabilities = $false }
    Write-Json (Join-Path $out 'host.json') $hostInfo

    $ordinaryNames = @('arithmetic', 'address-plan', 'limits', 'resume', 'scalar', 'loads', 'stack-memory', 'memory-flags',
        'call-return', 'private-code', 'static-workers', 'private-data', 'shared', 'regular', 'identity', 'signals', 'futex', 'churn',
        'negative-change', 'negative-pressure', 'native-chain', 'memory-recovery', 'elf-baseline')
    $stateNames = @('arithmetic', 'address-plan', 'loads', 'stack-memory', 'memory-flags', 'call-return', 'scalar')
    $signalNames = @('caught-ud2', 'caught-ud2-recursive', 'caught-xgetbv', 'uncaught-ud2', 'cpu-policy')
    $fixturePaths = @(
        foreach ($name in $ordinaryNames) { Join-Path $ordinary "$name.elf" }
        Join-Path $ordinary 'stop.elf'
        Join-Path $cpu 'cpu-0.elf'; Join-Path $cpu 'corpus.json'; Join-Path $signals 'manifest.json'
        foreach ($name in $signalNames) { Join-Path $signals "$name.elf" }
    )
    foreach ($file in $fixturePaths) {
        if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Transfer Linux-prepared fixture/manifest first: $file" }
        if ($file.EndsWith('.elf')) { Assert-Elf $file }
    }
    $cpuInputs = Get-CpuInputs $cpu
    $cpuCases = @($cpuInputs.cases)
    $summary.cpu_normal_cases = $cpuCases.Count; $summary.cpu_native_captures = $cpuInputs.native_captures
    $summary.cpu_pins_sha256 = $cpuInputs.pins_sha256
    $signalManifest = Get-Content -LiteralPath (Join-Path $signals 'manifest.json') -Raw | ConvertFrom-Json -AsHashtable
    foreach ($name in $signalNames) {
        if ((Get-Hash (Join-Path $signals "$name.elf")) -cne $signalManifest[$name].sha256) { throw "Signal manifest SHA256 mismatch: $name" }
    }
    $contractDirectory = Join-Path $source 'blink/tests/GuestJit/ContractTests'
    if (-not (Test-Path -LiteralPath (Join-Path $contractDirectory 'FullNativeDispatcherTests.cs')) -or
        (Get-Content -LiteralPath (Join-Path $contractDirectory 'Program.cs') -Raw) -notmatch 'CheckFullNativeDispatcher\s*\(\s*machine\s*\)') {
        throw 'ContractTests must include and invoke the new FullNativeDispatcherTests.'
    }
    $sourceBefore = @(Get-SourceInventory); $fixturesBefore = @(Get-FixtureInventory)
    Write-Json (Join-Path $out 'source-before.json') $sourceBefore
    Write-Json (Join-Path $out 'fixtures-before.json') $fixturesBefore
    $dotnet = (Get-Command dotnet.exe -CommandType Application -ErrorAction Stop).Source
    Assert-WindowsPe $dotnet
    Write-Json (Join-Path $out 'dotnet-identity.json') (Get-BinaryIdentity $dotnet)
    [void](Invoke-Recorded -Label 'dotnet-info' -Argv @($dotnet, '--info') -WorkingDirectory $source)

    $executions = @('InProcess', 'SeparateProcess')
    $publicKeys = @(foreach ($execution in $executions) { foreach ($name in $ordinaryNames) {
        foreach ($mode in @('Interpreter', 'Full')) { "$name|$execution|$mode" }
    } })
    $publicKeys += @(foreach ($execution in $executions) {
        foreach ($name in @('arithmetic', 'elf-baseline')) { "$name/SIMULATED-baseline-host-mask|$execution|Full" }
        foreach ($name in @('cooperative-stop', 'deadline', 'small-cache-recovery')) { "$name|$execution|Full" }
    })
    $publicKeys += @(foreach ($index in 0..3) { "moving-gc-owner-$index|InProcess|Full" })
    $stateKeys = @(foreach ($name in $stateNames) { foreach ($budget in 1..65) { "$name/$budget|Full" } })
    $signalKeys = @(foreach ($execution in $executions) { foreach ($name in $signalNames) {
        foreach ($mode in @('Interpreter', 'Hybrid', 'Full')) { "$name|$execution|$mode" }
    } })
    foreach ($form in @('processed', 'raw')) {
        $productName = if ($form -eq 'raw') { 'TranslatedBlink.Raw' } else { 'TranslatedBlink' }
        $product = Join-Path $source "blink/generated/$productName/TranslatedBlink.csproj"
        if (-not (Test-Path -LiteralPath $product -PathType Leaf)) { throw "Missing frozen $form generated closure: $product" }
        foreach ($runtime in @('coreclr', 'nativeaot')) {
            $prefix = "$form-$runtime"
            $deployments = @{}
            foreach ($application in @('FullJit', 'FullJitSignals', 'ContractTests')) {
                $project = if ($application -eq 'ContractTests') { Join-Path $contractDirectory 'ContractTests.csproj' }
                    else { Join-Path $source "blink/tests/$application/$application.csproj" }
                $publish = Join-Path $out "publish/$prefix/$application"
                $artifacts = Join-Path $out "build/$prefix/$application"
                $aot = if ($runtime -eq 'nativeaot') { 'true' } else { 'false' }
                $selfContained = if ($runtime -eq 'nativeaot') { 'true' } else { 'false' }
                $buildArgs = @($dotnet, 'publish', $project, '-c', 'Release', '-r', 'win-x64', '--self-contained', $selfContained,
                    '--disable-build-servers', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:BuildInParallel=false',
                    '-p:IlcSingleThreaded=true', '-p:UseAppHost=true', '-p:BlinkDeployWorker=true', "-p:BlinkProject=$product",
                    "-p:PublishAot=$aot", '--artifacts-path', $artifacts, '-o', $publish)
                [void](Invoke-Recorded -Label "$prefix-$application-publish" -Argv $buildArgs -WorkingDirectory $source -TimeoutSeconds 3600)
                $exe = Join-Path $publish "$application.exe"
                Assert-WindowsPe $exe
                $launch = if ($runtime -eq 'nativeaot') { @($exe) } else { @($dotnet, (Join-Path $publish "$application.dll")) }
                $workerExe = ''
                $roots = @(@{ directory = $publish; name = $application })
                if ($application -ne 'ContractTests') {
                    $workerDirectory = Join-Path $publish 'blink-worker'
                    $workerExe = Join-Path $workerDirectory 'Managed.Emulation.Worker.exe'
                    Assert-WindowsPe $workerExe
                    $roots += @{ directory = $workerDirectory; name = 'Managed.Emulation.Worker' }
                }
                foreach ($root in $roots) {
                    $dll = Join-Path $root.directory "$($root.name).dll"
                    $config = Join-Path $root.directory "$($root.name).runtimeconfig.json"
                    if ($runtime -eq 'coreclr') {
                        if (-not (Test-Path -LiteralPath $dll) -or -not (Test-Path -LiteralPath $config)) { throw "Missing matching CoreCLR controller/worker closure: $($root.directory)" }
                    } elseif ((Test-Path -LiteralPath $dll) -or (Test-Path -LiteralPath $config)) {
                        throw "Unexpected managed controller/worker in NativeAOT publish: $($root.directory)"
                    }
                }
                Write-Json (Join-Path $out "reports/$prefix-$application-binaries.json") @(
                    foreach ($file in (Get-ChildItem -LiteralPath $publish -File -Recurse | Sort-Object FullName)) { Get-BinaryIdentity $file.FullName }
                )
                $deployments[$application] = @{ launch = $launch; worker = $workerExe }
            }
            $jobs = @(
                @{ name = 'public'; args = @('public', $ordinary); keys = $publicKeys; suite = 'public'; worker = $true }
                @{ name = 'tripwire'; args = @('tripwire', $ordinary); keys = @(); suite = 'tripwire'; worker = $false }
                @{ name = 'state'; args = @('state', $ordinary); keys = $stateKeys; suite = 'state'; worker = $false }
                foreach ($execution in $executions) {
                    @{ name = "cpu-$execution"; args = @('cpu', $cpu); execution = $execution; suite = 'cpu'; worker = ($execution -eq 'SeparateProcess')
                        keys = @(foreach ($case in $cpuCases) { foreach ($mode in @('Interpreter', 'Full')) { "$($case.name)|$execution|$mode" } }) }
                }
                @{ name = 'signals'; args = @($signals); keys = $signalKeys; suite = 'signals'; worker = $true }
            )
            foreach ($job in $jobs) {
                $application = if ($job.suite -eq 'signals') { 'FullJitSignals' } else { 'FullJit' }
                $deployment = $deployments[$application]
                $report = Join-Path $out "reports/$prefix-$($job.name).json"
                $arguments = @($deployment.launch) + @($job.args) + @($report)
                if ($job.suite -eq 'cpu') { $arguments += $job.execution }
                $marker = if ($job.suite -eq 'signals') { '(?m)^PASS FullJitSignals observations=30\r?$' }
                    else { "(?m)^PASS FullJit $($job.suite) rows=$($job.keys.Count)\r?`$" }
                $record = Invoke-Recorded -Label "$prefix-$($job.name)" -Argv $arguments -ExpectedMarker $marker -WorkerExecutable $deployment.worker -RequireWorker:$job.worker
                $count = Assert-Report $report $job.keys $job.suite
                $record.status = 'PASS'; $record.report_sha256 = Get-Hash $report
                $cells.Add(@{ form = $form; host_runtime = $runtime; suite = $job.name; status = 'PASS'; observations = $count; report = $report })
                Save-Receipts
            }
            $record = Invoke-Recorded -Label "$prefix-contract" -Argv $deployments.ContractTests.launch -ExpectedMarker '(?m)^PASS emitter contract checks=[1-9][0-9]*\r?$'
            $record.status = 'PASS'
            $cells.Add(@{ form = $form; host_runtime = $runtime; suite = 'native-contract-including-FullNativeDispatcherTests'; status = 'PASS' })
            Save-Receipts
        }
    }
    $matrixComplete = $cells.Count -eq 28
} catch {
    $summary.status = if ($_.Exception.Data.Contains('QualificationStatus')) { $_.Exception.Data['QualificationStatus'] } else { 'FAILED' }
    $summary.error = $_.Exception.Message
    throw
} finally {
    try {
        if ($null -ne $sourceBefore) {
            $sourceAfter = @(Get-SourceInventory)
            Write-Json (Join-Path $out 'source-after.json') $sourceAfter
            $summary.source_stable = (ConvertTo-Json -InputObject $sourceBefore -Depth 6 -Compress) -ceq (ConvertTo-Json -InputObject $sourceAfter -Depth 6 -Compress)
        }
        if ($null -ne $fixturesBefore) {
            $fixturesAfter = @(Get-FixtureInventory)
            Write-Json (Join-Path $out 'fixtures-after.json') $fixturesAfter
            $summary.fixtures_stable = (ConvertTo-Json -InputObject $fixturesBefore -Depth 6 -Compress) -ceq (ConvertTo-Json -InputObject $fixturesAfter -Depth 6 -Compress)
        }
        if ($matrixComplete -and $summary.source_stable -and $summary.fixtures_stable) {
            $summary.status = 'WINDOWS_PASS'; $summary.windows_pass = $true
        } elseif ($summary.status -eq 'RUNNING') {
            $summary.status = 'INCOMPLETE_OR_INPUTS_CHANGED'
        }
    } catch {
        $summary.status = 'INPUT_VERIFICATION_FAILED'; $summary.windows_pass = $false
        $summary.error = $_.Exception.Message
    } finally {
        try {
            $summary.finished_utc = [DateTime]::UtcNow.ToString('o')
            Save-Receipts
        } finally {
            foreach ($name in $environmentBackup.Keys) { [Environment]::SetEnvironmentVariable($name, $environmentBackup[$name], 'Process') }
        }
    }
}
if (-not $summary.windows_pass) { Fail $summary.status 'Windows qualification did not pass; inspect qualification.json and commands.json.' }
Write-Output "WINDOWS_PASS: all 24 suite cells completed on this actual Windows x64 host; evidence in $out"
