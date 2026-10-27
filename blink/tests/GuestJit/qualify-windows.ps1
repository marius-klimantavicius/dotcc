param(
    [Parameter(Mandatory=$true)][string]$FixturesDirectory,
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [string]$SourceRoot = (Resolve-Path "$PSScriptRoot/../../..").Path,
    [switch]$SkipNativeAot
)
$ErrorActionPreference = 'Stop'
if (-not $IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne 'X64') {
    throw 'This qualification requires a real Windows x64 executing host.'
}
$fixtures = (Resolve-Path $FixturesDirectory).Path
foreach ($fixture in @('private-data','memory-flags','call-return')) {
    if (-not (Test-Path (Join-Path $fixtures "$fixture.elf"))) {
        throw "Prepare $fixture.elf with the Linux runner and its native witnesses before qualification."
    }
}
$out = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force $out | Out-Null
$commands = [Collections.Generic.List[object]]::new()
function Invoke-Checked([string]$Label, [string[]]$CommandLine) {
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $stdout = Join-Path $out "$Label.stdout"
    $stderr = Join-Path $out "$Label.stderr"
    $executable = $CommandLine[0]
    $arguments = $CommandLine[1..($CommandLine.Length-1)]
    & $executable @arguments 1> $stdout 2> $stderr
    $code = $LASTEXITCODE
    $commands.Add(@{label=$Label;argv=$CommandLine;exit_code=$code;wall_ms=$watch.Elapsed.TotalMilliseconds})
    $commands | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $out 'commands.json')
    if ($code -ne 0) { throw "$Label exited $code; see $stdout and $stderr" }
}
@{os=[Runtime.InteropServices.RuntimeInformation]::OSDescription;runtime=[Runtime.InteropServices.RuntimeInformation]::FrameworkDescription;
    architecture=[Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString();windows_verified=$false;
    fixtures=(Get-ChildItem "$fixtures/*.elf" | ForEach-Object { @{name=$_.Name;sha256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()} })
} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $out 'environment.json')
$project = Join-Path $SourceRoot 'blink/tests/GuestJit/GuestJit.csproj'
$contractProject = Join-Path $SourceRoot 'blink/tests/GuestJit/ContractTests/ContractTests.csproj'
foreach ($form in @('processed','raw')) {
    $productName = if ($form -eq 'raw') { 'TranslatedBlink.Raw' } else { 'TranslatedBlink' }
    $product = Join-Path $SourceRoot "blink/generated/$productName/TranslatedBlink.csproj"
    if (-not (Test-Path $product)) { throw 'Prepare a frozen source closure with the current translated producer on Linux first.' }
    $common = @('-c','Release','--disable-build-servers','-m:1','-p:UseSharedCompilation=false','-p:BuildInParallel=false',"-p:BlinkProject=$product")
    $coreclr = Join-Path $out "$form-coreclr"
    Invoke-Checked "$form-build" (@('dotnet','build',$project) + $common + @('-o',$coreclr))
    Invoke-Checked "$form-coreclr-qualify" @('dotnet',"$coreclr/GuestJit.dll",'qualify',$fixtures,"$out/$form-coreclr.json")
    $contractCoreClr = Join-Path $out "$form-contract-coreclr"
    Invoke-Checked "$form-contract-build" (@('dotnet','build',$contractProject) + $common + @('-o',$contractCoreClr))
    Invoke-Checked "$form-contract-coreclr" @('dotnet',"$contractCoreClr/ContractTests.dll")
    if (-not $SkipNativeAot) {
        $aot = Join-Path $out "$form-nativeaot"
        Invoke-Checked "$form-publish" (@('dotnet','publish',$project) + $common + @('-r','win-x64','-p:PublishAot=true','-o',$aot))
        Invoke-Checked "$form-nativeaot-qualify" @("$aot/GuestJit.exe",'qualify',$fixtures,"$out/$form-nativeaot.json")
        $contractAot = Join-Path $out "$form-contract-nativeaot"
        Invoke-Checked "$form-contract-publish" (@('dotnet','publish',$contractProject) + $common + @('-r','win-x64','-p:PublishAot=true','-o',$contractAot))
        Invoke-Checked "$form-contract-nativeaot" @("$contractAot/ContractTests.exe")
    }
}
Write-Output "Qualification finished; inspect native instruction counts and mitigation capability results in $out before declaring any Windows profile passed."
