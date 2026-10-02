"""Native destruction traces compared with managed/NativeAOT exception injection."""
import json
import os
import resource
import subprocess
import sys


def verify_lifetimes(root, tests, out, nativeaot, run):
    checks = []
    oracle = out / 'native-lifetimes'
    run(['g++', '-std=c++17', '-O0', tests / 'lifetimes_native.cpp', '-o', oracle])
    expected = run([oracle]).stdout
    (out / 'native-lifetime-trace.txt').write_text(expected)
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / f'lifetimes-{mode}'
        options = ['--instance-methods'] if instance else []
        run([sys.executable, root / 'dotnet.py', tests / 'lifetimes.cpp', '--emit', 'managedlib',
             '--output-dir', directory, '--class-name', 'Core', '--namespace', '',
             '--split', 'function', *options])
        symbols = json.loads((directory / 'dot-gen.json').read_text())['exports']
        def method(prefix):
            found = [x['method'] for x in symbols if x['symbol'].startswith(prefix)]
            assert len(found) == 1, (prefix, symbols)
            return ('owner.' if instance else 'Core.') + found[0]
        reset, read = method('_Z5reset'), method('_Z10read_trace')
        nested, members = method('_Z6nested'), method('_Z7members')
        loops, jump = method('_Z5loops'), method('_Z4jump')
        backward, throwing = method('_Z12reverse_jump'), method('_Z13throwing_dtor')
        no_throw = method('_Z8no_throw')
        host = directory / 'host'
        host.mkdir(exist_ok=True)
        (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        code = f'''using System;
unsafe class EntryPoint {{
    static int fail;
    static readonly Exception injected = new InvalidOperationException("injected");
    {('static readonly Core owner = new Core();' if instance else '')}
    static void Hook({'Core current, ' if instance else ''}int value) {{
        if(value==fail || (fail==98 && (value==9 || value==8))) throw injected;
    }}
    static void Show(string label,Action action,int at) {{
        fail=at; {reset}(); int caught=0;
        try {{action();}} catch(Exception ex) {{
            if(!Object.ReferenceEquals(ex,injected)) throw;
            caught=at;
        }}
        Console.WriteLine($"{{label}}:{{caught}}:{{{read}()}}");
    }}
    static int Main(string[] args) {{
        if(args.Length!=0) {{
            fail=args[0]=="noexcept" ? 9 : 98;
            try {{ if(args[0]=="noexcept") {no_throw}(&Hook); else {throwing}(&Hook); }}
            catch(Exception) {{ return 91; }} // Force CLR unwinding to a real handler.
            return 90;
        }}
        Show("nested",()=>{nested}(&Hook,0),0);
        Show("early",()=>{nested}(&Hook,1),0);
        Show("first-ctor",()=>{nested}(&Hook,0),1);
        Show("second-ctor",()=>{nested}(&Hook,0),2);
        Show("body",()=>{nested}(&Hook,0),9);
        Show("members",()=>{members}(&Hook),0);
        Show("member-ctor",()=>{members}(&Hook),2);
        Show("outer-ctor",()=>{members}(&Hook),3);
        Show("member-body",()=>{members}(&Hook),9);
        Show("loops",()=>{loops}(&Hook),0);
        Show("jump",()=>{jump}(&Hook),0);
        Show("jump-throw",()=>{jump}(&Hook),9);
        Show("backward",()=>{backward}(&Hook),0);
        Show("throwing-dtor",()=>{throwing}(&Hook),8);
        return 0;
    }}
}}'''
        (host / 'Program.cs').write_text(code)
        run(['dotnet','build',host / 'Host.csproj','-c','Release','--nologo'])
        commands = [('JIT',['dotnet',host / 'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host / 'Host.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host / 'aot'])
            commands.append(('NativeAOT',[host / 'aot/Host']))
        for runtime, command in commands:
            actual=run(command).stdout
            assert actual==expected, f'{mode}/{runtime}\nexpected:\n{expected}\nactual:\n{actual}'
            (host / f'{runtime}-trace.txt').write_text(actual)
            checks.append(f'lifetimes/{mode}/{runtime}: 14 exact native destruction traces and exception identity')
            for argument, message in [('noexcept','Exception escaped noexcept function'),
                                      ('double','Destructor threw during unwinding')]:
                # These child processes must terminate. Avoid generating core dumps.
                result=subprocess.run([*map(str,command),argument],capture_output=True,text=True,
                                      preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
                assert result.returncode not in (0,90,91) and message in result.stderr, result.stderr
            checks.append(f'lifetimes/{mode}/{runtime}: noexcept and double-exception termination')
    # Audit the real lowering boundary with EH disabled, without adding a
    # public driver option that would promise foreign-exception cleanup then.
    directory=out / 'lifetime-noeh'
    run([sys.executable, root / 'dotnet.py', tests / 'lifetime_normal.cpp', '--output-dir', directory])
    env=dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc'),
             EDG_DOT_NAMESPACE='@Edg.@Generated', EDG_DOT_CLASS='@TranslatedProgram',
             EDG_DOT_INSTANCE='0', EDG_DOT_TRACE='1')
    base=[root / 'build/bin/cpfe-dot','--c++17','--gnu_version','130300']
    enabled=run([*base,'--',tests / 'lifetime_normal.cpp'],env=env)
    disabled=run([*base,'--no_exceptions','--',tests / 'lifetime_normal.cpp'],env=env)
    assert 'descriptor=present' in enabled.stderr and 'cleanup=' in enabled.stderr
    assert 'cleanup=' not in disabled.stderr and '__edg_cleanup_state' not in disabled.stdout
    (directory / 'retained-eh.il.txt').write_text(enabled.stderr)
    (directory / 'disabled-eh.il.txt').write_text(disabled.stderr)
    (directory / 'Translated.eh-enabled.cs.saved').write_bytes((directory / 'Translated.cs').read_bytes())
    (directory / 'Translated.cs').write_text(disabled.stdout)
    # The direct EH-disabled compiler invocation can omit lifecycle services
    # that the normal driver invocation required for exception bookkeeping.
    # Keep the host consistent with the actual replacement compiler output.
    entry=directory / 'EntryPoint.cs'
    (directory / 'EntryPoint.eh-enabled.cs.saved').write_bytes(entry.read_bytes())
    entry.write_text('internal static class EntryPoint { public static int Main() { '+
                    ('try { return @Edg.@Generated.@TranslatedProgram.Run(); } finally { @Edg.@Generated.@TranslatedProgram.Shutdown(); }'
                     if 'void Shutdown()' in disabled.stdout else 'return @Edg.@Generated.@TranslatedProgram.Run();')+' } }\n')
    run(['g++','-std=c++17','-fno-exceptions',tests / 'lifetime_normal.cpp','-o',out / 'native-noeh'])
    run([out / 'native-noeh'])
    from runtime_verify import execute
    execute(directory,nativeaot,run,checks,'EH-disabled normal cleanup/native oracle')
    return checks
