"""Consumer tests for independent instances of the unchanged SQLite engine."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    sqlite = root.parent / 'sqlite'
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    config = json.loads((sqlite / 'config/sources.json').read_text())['product']
    amalgamation = sqlite / 'ref' / config['directory']
    definitions = [line.strip() for line in (sqlite / 'config/defines.txt').read_text().splitlines()
                   if line.strip() and not line.lstrip().startswith('#')]
    sources = [amalgamation / 'sqlite3.c', Path(__file__).with_name('sqlite_project_owners.c'),
               sqlite / 'tests/memory_vfs.c']
    report = {'status': 'running', 'checks': [], 'commands': [], 'definitions': definitions,
              'profile': 'full configured SQLite, test-only memory VFS, independent owners; THREADSAFE=0',
              'sources': [{'path': str(p), 'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in sources]}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command):
        command = list(map(str, command))
        report['commands'].append({'label': label, 'command': command})
        save()
        stdout = stderr = b''
        try:
            result = subprocess.run(command, cwd=out, capture_output=True, timeout=1200)
            stdout, stderr = result.stdout, result.stderr
            report['commands'][-1]['exitCode'] = result.returncode
        except subprocess.TimeoutExpired as error:
            stdout, stderr = error.stdout or b'', error.stderr or b''
            report['commands'][-1]['timedOut'] = True
            raise
        finally:
            (out / (label + '.stdout')).write_bytes(stdout)
            (out / (label + '.stderr')).write_bytes(stderr)
            save()
        if result.returncode:
            raise RuntimeError(f'{label} failed: {result.returncode}; see retained logs')

    try:
        generated = out / 'managed'
        run('translate', [sys.executable, root / 'dotnet.py', *sources, '--language', 'c', '--std', 'c17',
                          '--libc', 'musl', '-I', amalgamation, '-I', sqlite / 'tests',
                          *['-D' + item for item in definitions], '--emit', 'managedlib',
                          '--namespace', '', '--class-name', 'SqliteCore', '--instance-methods',
                          '--split', 'size', '--output-dir', generated])
        manifest = json.loads((generated / 'dot-gen.json').read_text())
        names = {item['symbol']: item['method'] for item in manifest['exports']}
        host = out / 'host'
        host.mkdir()
        (host / 'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>'
            '<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>'
            '<AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>'
            '<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup>'
            '<ProjectReference Include="../managed/Generated.csproj" /></ItemGroup></Project>')
        code = '''using System; using System.Threading;
unsafe class Program {
 static void Require(bool value,string detail) { if(!value) throw new Exception(detail); }
 static void Sql(SqliteCore core,ReadOnlySpan<byte> sql) {
  fixed(byte* p=sql) Require(core.PROJECT_EXECUTE((sbyte*)p)==0,"SQL statement");
 }
 static void Main() {
  using var first=new SqliteCore(); using var second=new SqliteCore();
  Require(first.PROJECT_IS_OPEN()==0 && second.PROJECT_IS_OPEN()==0,"fresh globals");
  Require(first.PROJECT_OPEN(7)==0,"first open");
  Require(second.PROJECT_IS_OPEN()==0,"owner global alias");
  Require(second.PROJECT_OPEN(100)==0,"second open");
  Sql(first,"BEGIN; INSERT INTO values_table VALUES(11)"u8);
  Require(first.PROJECT_SUM()==18 && second.PROJECT_SUM()==100,"transaction isolation");
  Sql(second,"INSERT INTO values_table VALUES(23)"u8); Sql(first,"ROLLBACK"u8);
  Require(first.PROJECT_SUM()==7 && second.PROJECT_SUM()==123,"rollback isolation");
  GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
  Require(first.PROJECT_SUM()==7 && second.PROJECT_SUM()==123,"pinned global storage");
  Exception? failure=null;
  // No overlapping access to a THREADSAFE=0 engine. Move each owner to a
  // worker sequentially and release only that worker's translated TLS.
  foreach(var core in new[]{first,second}) {
   var thread=new Thread(()=>{try{Sql(core,"INSERT INTO values_table VALUES(5)"u8);core.ReleaseThread();}
                              catch(Exception e){failure=e;}});
   thread.Start();thread.Join();if(failure!=null)throw failure;
  }
  Require(first.PROJECT_SUM()==12 && second.PROJECT_SUM()==128,"thread handoff");
  Require(first.PROJECT_CLOSE()==0,"first close"); first.Shutdown();
  Require(second.PROJECT_SUM()==128,"survivor after owner shutdown");
  Require(second.PROJECT_CLOSE()==0,"second close"); second.Shutdown();
  using var replacement=new SqliteCore();
  Require(replacement.PROJECT_IS_OPEN()==0 && replacement.PROJECT_OPEN(901)==0,"replacement owner");
  Require(replacement.PROJECT_SUM()==901 && replacement.PROJECT_CLOSE()==0,"replacement state");
  replacement.Shutdown();
  Console.WriteLine("SQLite independent globals, databases, transactions, GC, thread handoff and owner disposal passed");
 }
}
'''
        for symbol in ['project_execute', 'project_is_open', 'project_open', 'project_sum', 'project_close']:
            code = code.replace(symbol.upper(), names[symbol])
        (host / 'Program.cs').write_text(code)
        run('build', ['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo',
                      '-p:TreatWarningsAsErrors=true'])
        run('jit', ['dotnet', host / 'bin/Release/net10.0/Host.dll'])
        report['checks'].append('SQLite independent owners: JIT')
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                            '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run('nativeaot', [host / 'aot/Host'])
            report['checks'].append('SQLite independent owners: NativeAOT')
        report['status'] = 'passed'
    except Exception as error:
        report['status'] = 'failed'
        report['failure'] = str(error)
        raise
    finally:
        save()
    print(json.dumps(report['checks'], indent=2))


if __name__ == '__main__':
    main()
