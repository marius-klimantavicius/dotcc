#!/usr/bin/env python3
"""Real SDK/package generators and source-sensitive generators in both CLI modes."""
import argparse
import hashlib
import json
import pathlib
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--tool', required=True, type=pathlib.Path)
tool = parser.parse_args().tool.resolve()

def run(*command, expected=0):
    result = subprocess.run([str(x) for x in command], text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    assert result.returncode == expected, f'{command}: expected {expected}, got {result.returncode}\n{result.stdout}'
    return result.stdout

def build(project):
    run('dotnet', 'build', project, '-c', 'Release', '-v:q')

def execute(project, assembly):
    return run('dotnet', project.parent / f'bin/Release/net10.0/{assembly}.dll')

with tempfile.TemporaryDirectory(prefix='dotcc-generators-tests-') as temporary:
    root = pathlib.Path(temporary)
    source = root / 'input'
    source.mkdir()
    project = source / 'Sample.csproj'
    project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup><ItemGroup><PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.2" /></ItemGroup></Project>''')
    code = source / 'Program.cs'
    code.write_text('''using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
static class Cond { public static bool B(int x) => x != 0; }
public record Person(string Name, int Age);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Person))]
internal partial class PersonContext : JsonSerializerContext { }
internal static partial class Native {
 [LibraryImport("libc", EntryPoint = "getpid")] internal static partial int GetPid();
 [LibraryImport("kernel32", EntryPoint = "GetCurrentProcessId")] internal static partial uint GetWindowsPid();
 [LibraryImport("libc", EntryPoint = "strlen", StringMarshalling = StringMarshalling.Utf8)]
 internal static partial nuint Length(string value);
}
internal static partial class Log {
 [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Person {Name}")]
 internal static partial void Person(ILogger logger, string name);
}
sealed class Capture : ILogger {
 public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
 public bool IsEnabled(LogLevel level) => true;
 public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> format)
  => Console.WriteLine($"{id.Id}:{format(state, error)}");
}
static class Program {
 static void Main() {
  { /* cleanup */ }
  bool native = OperatingSystem.IsWindows() ? Native.GetWindowsPid() > 0 : Native.GetPid() > 0 && Native.Length("hello") == 5;
  Console.WriteLine(Cond.B(native ? 1 : 0));
  var json = JsonSerializer.Serialize(new Person("Ada", 37), PersonContext.Default.Person);
  Console.WriteLine(json);
  Log.Person(new Capture(), JsonSerializer.Deserialize(json, PersonContext.Default.Person)!.Name);
 }
}
''')
    build(project)
    expected = execute(project, 'Sample')
    assert expected == 'True\n{"name":"Ada","age":37}\n7:Person Ada\n', expected
    before = code.read_bytes()
    # Existing generated files may be stale; only fresh in-memory generation is authoritative.
    generated = list((source / 'obj').rglob('*.g.cs'))
    assert any('LibraryImports' in str(path) for path in generated), generated
    generated_bytes = {path: path.read_bytes() for path in generated}
    for path in generated:
        if 'generated' in path.parts:
            path.write_text('// stale on-disk generator output\n')
    stale = {path: path.read_bytes() for path in generated}
    output = root / 'snapshot'
    run('dotnet', tool, project, '--output', output)
    manifest = json.loads((output / 'manifest.json').read_text())
    assert manifest['Rewritten'] == 1, manifest
    assert manifest['RemovedEmptyBlocks'] == 1, manifest
    assert code.read_bytes() == before
    assert all(path.read_bytes() == data for path, data in stale.items())
    generator_names = '\n'.join(item['Generator'] for item in manifest['GeneratedSources'])
    for name in ('LibraryImport', 'LoggerMessage', 'JsonSourceGenerator'):
        assert name in generator_names, generator_names
    for item in manifest['Files']:
        assert hashlib.sha256((output / item['Path']).read_bytes()).hexdigest() == item['Sha256']
    for phase in ('Original', 'Optimized'):
        for generated in (item for item in manifest['GeneratedSources'] if item['Phase'] == phase):
            path = next(path for path in (output / phase / 'src').rglob(pathlib.Path(generated['Path']).name)
                        if hashlib.sha256(path.read_bytes().decode('utf-8-sig').encode()).hexdigest() == generated['Sha256'])
            assert path.exists()
    for variant in ('OriginalProject', 'OptimizedProject'):
        snapshot = output / manifest[variant]
        build(snapshot)
        assert execute(snapshot, 'Sample') == expected
    output2 = root / 'snapshot2'
    run('dotnet', tool, project, '--output', output2)
    assert (output / 'manifest.json').read_bytes() == (output2 / 'manifest.json').read_bytes()
    assert 'Updated 1 source files in place.' in run('dotnet', tool, project, '--in-place')
    assert all(path.read_bytes() == data for path, data in stale.items())
    build(project)
    assert execute(project, 'Sample') == expected
    unchanged = code.read_bytes(), code.stat().st_mtime_ns
    assert 'Updated 0 source files in place.' in run('dotnet', tool, project, '--in-place')
    assert unchanged == (code.read_bytes(), code.stat().st_mtime_ns)
    # The generator diagnostic is an error even if the declared partial void could be erased.
    code.write_text(code.read_text().replace('internal static partial void Person', 'internal static partial int Person'))
    invalid = code.read_bytes()
    failed = root / 'failed'
    message = run('dotnet', tool, project, '--output', failed, expected=1)
    assert 'SYSLIB1007' in message, message
    assert not failed.exists()
    run('dotnet', tool, project, '--in-place', expected=1)
    assert code.read_bytes() == invalid

    # A local generator exercises configuration, AdditionalFiles, regeneration,
    # changing document sets, and errors which are independent of C# diagnostics.
    generator = root / 'generator'
    generator.mkdir()
    genproject = generator / 'Generator.csproj'
    genproject.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup><ItemGroup>
<Reference Include="Microsoft.CodeAnalysis" HintPath="$(MSBuildBinPath)/Roslyn/bincore/Microsoft.CodeAnalysis.dll" />
</ItemGroup></Project>''')
    (generator / 'Generator.cs').write_text('''using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
[Generator] public class Generator : ISourceGenerator {
 public void Initialize(GeneratorInitializationContext context) { }
 public void Execute(GeneratorExecutionContext context) {
  context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GeneratorMode", out var mode);
  if (mode == "throw") throw new InvalidOperationException("generator exploded");
  if (mode == "error") { context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("TEST001", "Error", "generator rejected input", "Test", DiagnosticSeverity.Error, true), Location.None)); return; }
  if (mode == "warning") context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("TEST002", "Warning", "generator warning", "Test", DiagnosticSeverity.Warning, true), Location.None));
  var file = context.AdditionalFiles.Single();
  context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.AdditionalFiles.Flavor", out var flavor);
  bool rewritten = !context.Compilation.SyntaxTrees.Any(tree => tree.ToString().Contains("Cond.B("));
  context.AddSource("Generated.g.cs", SourceText.From("public static class Generated { public const string Value = \\\"" + file.GetText()!.ToString().Trim() + "-" + flavor + "-" + mode + "\\\"; public const bool Rewritten = " + rewritten.ToString().ToLowerInvariant() + "; public static bool Identity(bool x) { {} return x == false ? false : true; } }", Encoding.UTF8));
  if (!rewritten) context.AddSource("Before.g.cs", "internal class Before {}");
 }
}''')
    build(genproject)
    custom = root / 'custom'
    custom.mkdir()
    customproj = custom / 'Custom.csproj'
    customproj.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><GeneratorMode>normal</GeneratorMode></PropertyGroup><ItemGroup>
<Analyzer Include="../generator/bin/Release/net10.0/Generator.dll" />
<CompilerVisibleProperty Include="GeneratorMode" /><AdditionalFiles Include="value.txt" Flavor="metadata" />
<CompilerVisibleItemMetadata Include="AdditionalFiles" MetadataName="Flavor" />
</ItemGroup></Project>''')
    (custom / 'value.txt').write_text('additional')
    customcode = custom / 'Program.cs'
    customcode.write_text('''static class Cond { public static bool B(int x) => x != 0; }
static class Program { static void Main() { System.Console.WriteLine(Cond.B(1)); System.Console.WriteLine(Generated.Value + "-" + Generated.Rewritten); } }''')
    build(customproj)
    originalcustom = customcode.read_bytes()
    customout = root / 'custom-snapshot'
    run('dotnet', tool, customproj, '--output', customout)
    cm = json.loads((customout / 'manifest.json').read_text())
    assert len([s for s in cm['GeneratedSources'] if s['Phase'] == 'Original']) == 2
    assert len([s for s in cm['GeneratedSources'] if s['Phase'] == 'Optimized']) == 1
    assert cm['RemovedEmptyBlocks'] == 0 and cm['SimplifiedBooleanComparisons'] == 0, cm
    for variant, rewritten in (('OriginalProject', 'False'), ('OptimizedProject', 'True')):
        snapshot = customout / cm[variant]
        build(snapshot)
        assert execute(snapshot, 'Custom') == f'True\nadditional-metadata-normal-{rewritten}\n'
    assert 'Updated 1 source files in place.' in run('dotnet', tool, customproj, '--in-place')
    assert not list(custom.glob('*.g.cs'))
    build(customproj)
    assert execute(customproj, 'Custom') == 'True\nadditional-metadata-normal-True\n'
    customcode.write_bytes(originalcustom)
    config = customproj.read_text()
    for mode, diagnostic in (('error', 'TEST001'), ('throw', 'generator exploded'), ('warning', 'TEST002')):
        customproj.write_text(config.replace('<GeneratorMode>normal</GeneratorMode>', f'<GeneratorMode>{mode}</GeneratorMode><TreatWarningsAsErrors>true</TreatWarningsAsErrors>'))
        destination = root / ('rejected-' + mode)
        message = run('dotnet', tool, customproj, '--output', destination, expected=1)
        assert diagnostic in message, message
        assert not destination.exists()
        run('dotnet', tool, customproj, '--in-place', expected=1)
        assert customcode.read_bytes() == originalcustom
    # Failure only after rewriting must also leave authored sources untouched.
    genfile = generator / 'Generator.cs'
    genfile.write_text(genfile.read_text().replace('if (!rewritten) context.AddSource', 'if (rewritten) throw new InvalidOperationException("rewritten input rejected"); if (!rewritten) context.AddSource'))
    build(genproject)
    customproj.write_text(config)
    message = run('dotnet', tool, customproj, '--in-place', expected=1)
    assert 'rewritten input rejected' in message, message
    assert customcode.read_bytes() == originalcustom

print('PASS: LibraryImport, LoggerMessage, JSON, regeneration, configuration/additional files, diagnostics and in-place safety')
