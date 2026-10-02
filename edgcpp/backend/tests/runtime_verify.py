"""Runtime ABI checks with a separately compiled C# consumer and real threads."""
import json
import sys


def verify_runtime(root, tests, out, nativeaot, run):
    checks = []
    run(["g++", "-std=c++17", tests / "functions_literals.cpp", "-o", out / "native-functions-literals"])
    run([out / "native-functions-literals"])
    for instance in (False, True):
        mode = "instance" if instance else "static"
        options = ["--instance-methods"] if instance else []
        directory = out / f"functions-literals-{mode}"
        run([sys.executable, root / "dotnet.py", tests / "functions_literals.cpp",
             "--output-dir", directory, "--split", "function", *options])
        execute(directory, nativeaot, run, checks, f"function pointers and literals/{mode}")
        for nested in (False, True):
            directory = out / f"runtime-library-{mode}-{nested}"
            run([sys.executable, root / "dotnet.py", tests / "runtime_library.cpp",
                 "--output-dir", directory, "--emit", "managedlib", "--namespace", "",
                 "--class-name", "Core", "--split", "size", "--split-size", "1000",
                 "--nest-types" if nested else "--no-nest-types", *options])
            manifest = json.loads((directory / "dot-gen.json").read_text())
            assert not (directory / "EntryPoint.cs").exists()
            exports = {e["symbol"]: e["method"] for e in manifest["exports"]}
            def method(prefix):
                matches = [v for k, v in exports.items() if k.startswith(prefix)]
                assert len(matches) == 1, (prefix, exports)
                return matches[0]
            get = method("_Z12get_callback")
            call = method("_Z4call")
            tls = method("_Z8tls_next")
            address = method("_Z11tls_address")
            text = method("_Z12text_address")
            pair = method("_Z14pair_roundtrip")
            frame = method("_Z10frame_call")
            copy = method("_Z9pair_copy")
            a, b = ("a", "b") if instance else ("Core", "Core")
            owners = "var a=new Core(); var b=new Core();" if instance else ""
            invoke = f"fp({b}, 2)" if instance else "fp(2)"
            host = directory / "host"
            host.mkdir(exist_ok=True)
            (host / "Host.csproj").write_text('''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
            (host / "Program.cs").write_text(f'''using System;
using System.Threading;
unsafe class EntryPoint {{
    static int Fail({"Core owner, " if instance else ""}int value) => throw new InvalidOperationException();
    static int Identity({"Core owner, " if instance else ""}int value) => value;
    static int Main() {{
        {owners}
        nint firstCallback=0, secondCallback=0;
        using(var gate=new Barrier(2)) {{
            var t1=new Thread(()=>{{gate.SignalAndWait(); firstCallback=(nint){a}.{get}();}});
            var t2=new Thread(()=>{{gate.SignalAndWait(); secondCallback=(nint){b}.{get}();}});
            t1.Start(); t2.Start(); t1.Join(); t2.Join();
        }}
        if(firstCallback==0 || firstCallback!=secondCallback) return 14;
        var fp={a}.{get}();
        if((nuint)fp!=(nuint){b}.{get}()) return 1;
        if({invoke}!=12) return 2;
        if({a}.{call}(fp,3)!={13 if instance else 15}) return 3;
        if({a}.{tls}()!=11 || {a}.{tls}()!=12) return 4;
        {f'if({b}.{tls}()!=11) return 5;' if instance else ''}
        nint first=(nint){a}.{address}();
        nint other=0;
        int failed=0;
        var thread=new Thread(()=>{{
            if({a}.{tls}()!=11 || {a}.{tls}()!=12) failed=6;
            {f'if({b}.{tls}()!=11) failed=7;' if instance else ''}
            other=(nint){a}.{address}();
            for(int i=0;i<20000;i++) if((nuint)fp!=(nuint){a}.{get}()) failed=8;
        }});
        thread.Start();
        for(int i=0;i<20;i++) {{GC.Collect(2, GCCollectionMode.Forced,true,true);}}
        thread.Join();
        if(failed!=0) return failed;
        if(first==other || first!=(nint){a}.{address}() || {a}.{tls}()!=13) return 9;
        if({a}.{text}()[0]!=115 || {a}.{text}()[6]!=0 || {a}.{text}()[7]!=108) return 10;
        if((nuint){a}.{text}()!=(nuint){b}.{text}()) return 11;
        if({a}.{frame}(&Identity)!=88 || {a}.{copy}()!=8) return 12;
        for(int i=0;i<200;i++) {{
            try {{ {a}.{frame}(&Fail); return 13; }}
            catch(InvalidOperationException) {{ }}
        }}
        var p={a}.{pair}(default); // Proves public aggregate accessibility across assemblies.
        GC.KeepAlive(p);
        {('GC.KeepAlive(a); GC.KeepAlive(b);' if instance else '')}
        return 0;
    }}
}}''')
            execute(host, nativeaot, run, checks, f"TLS/owner/callback/library/{mode}/nested={nested}", "Host")
    return checks


def execute(directory, nativeaot, run, checks, label, name="Generated"):
    project = directory / f"{name}.csproj"
    run(["dotnet", "build", project, "-c", "Release", "--nologo"])
    run(["dotnet", directory / f"bin/Release/net10.0/{name}.dll"])
    checks.append(f"{label}: managed execution")
    if nativeaot:
        run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
             "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", directory / "aot"])
        run([directory / f"aot/{name}"])
        checks.append(f"{label}: linux-x64 NativeAOT execution")
