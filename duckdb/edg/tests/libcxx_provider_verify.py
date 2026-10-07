"""Native/JIT/NativeAOT acceptance for the real libc++ source assembly."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir",type=Path,required=True)
    parser.add_argument("--acquisition",type=Path,required=True)
    parser.add_argument("--nativeaot",action="store_true")
    args=parser.parse_args()
    root=Path(__file__).resolve().parents[3]
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    source=Path(__file__).with_name("libcxx_provider.cpp")
    spec=importlib.util.spec_from_file_location("duckdb_libcxx",Path(__file__).resolve().parents[1]/"libcxx.py")
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    provider=module.prepare_provider(root/"edgcpp",args.acquisition,out/"provider",standard="c++17",
                                    unity_provider=True,primary_consumers=[source])
    report={"passed":False,"provider":provider,"checks":[],"commands":[],
            "compilerSha256":hashlib.sha256((root/"edgcpp/build/bin/cpfe-dot").read_bytes()).hexdigest()}

    def run(stage,command):
        command=list(map(str,command));result=subprocess.run(command,capture_output=True,text=True)
        (out/(stage+".stdout")).write_text(result.stdout);(out/(stage+".stderr")).write_text(result.stderr)
        report["commands"].append({"stage":stage,"command":command,"exit":result.returncode})
        (out/"results.json").write_text(json.dumps(report,indent=2)+"\n")
        if result.returncode:raise RuntimeError(stage+" failed; see "+str(out))
        return result.stdout

    run("native-build",["g++","-std=c++17","-pthread",source,"-o",out/"native"])
    expected=run("native-run",[out/"native"])
    generated=out/"generated"
    command=[sys.executable,root/"edgcpp/dotnet.py",*provider["translationUnits"],"--std","c++17",
             "--libc","musl","--posix","--emit","exe","--output-dir",generated]
    for include in provider["includeDirectories"]:command.extend(["-I",include])
    run("translate",command)
    manifest=json.loads((generated/"dot-gen.json").read_text())
    if manifest.get("unboundExternals",{}).get("requiredPartials"):raise RuntimeError("Unresolved externs remain")
    report["checks"].append("Real LLVM18 provider + separate musl TUs translated with strict extern rejection; libc++ and C++ consumer share primary TU")
    run("build",["dotnet","build",generated/"Generated.csproj","-c","Release","--nologo"])
    if run("jit",["dotnet",generated/"bin/Release/net10.0/Generated.dll"])!=expected:raise RuntimeError("JIT differs from native")
    report["checks"].append("JIT: strings, streams, synchronized threads, regex, random device, vector exceptions, fifteen arithmetic sort instantiations, twenty floating formatting edge cases and short buffers")
    if args.nativeaot:
        run("publish",["dotnet","publish",generated/"Generated.csproj","-c","Release","-r","linux-x64",
                       "-p:PublishAot=true","-p:IlcTreatWarningsAsErrors=true","-o",generated/"aot"])
        if run("aot",[generated/"aot/Generated"])!=expected:raise RuntimeError("NativeAOT differs from native")
        report["checks"].append("NativeAOT: identical native-oracle output and behavior")
    report["passed"]=True
    (out/"results.json").write_text(json.dumps(report,indent=2)+"\n")


if __name__=="__main__":main()
