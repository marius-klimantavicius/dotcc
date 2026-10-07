"""Native/JIT/NativeAOT acceptance for the real libc++ source assembly."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import shutil


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir",type=Path,required=True)
    parser.add_argument("--acquisition",type=Path,required=True)
    parser.add_argument("--nativeaot",action="store_true")
    parser.add_argument("--separate-consumer",action="store_true",
                        help="exercise separate TUs with two explicitly recorded fixture-only retention anchors")
    args=parser.parse_args()
    root=Path(__file__).resolve().parents[3]
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    source=Path(__file__).with_name("libcxx_provider.cpp")
    spec=importlib.util.spec_from_file_location("duckdb_libcxx",Path(__file__).resolve().parents[1]/"libcxx.py")
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    compiler=out/"cpfe-dot"
    shutil.copy2(root/"edgcpp/build/bin/cpfe-dot",compiler)
    provider=module.prepare_provider(root/"edgcpp",args.acquisition,out/"provider",standard="c++17",
                                    unity_provider=True,primary_consumers=[] if args.separate_consumer else [source])
    units=provider["translationUnits"]
    fixture_inputs=[source]
    if args.separate_consumer:
        primary=out/"primary.cpp"
        anchors=Path(__file__).with_name("libcxx_provider_retention.cpp")
        primary.write_text('#include '+json.dumps(units[0])+'\n#include '+json.dumps(str(anchors))+'\n')
        consumer=out/"consumer.cpp"
        consumer.write_text(''.join('#include '+json.dumps(path)+'\n' for path in provider["preincludeHeaders"])+
                            '#include '+json.dumps(str(source))+'\n')
        units=[str(primary),str(consumer)]
        fixture_inputs += [anchors,primary,consumer]
    report={"passed":False,"provider":provider,"checks":[],"commands":[],
            "separateConsumer":args.separate_consumer,
            "fixtureInputs":{str(path):hashlib.sha256(path.read_bytes()).hexdigest() for path in fixture_inputs},
            "compilerSha256":hashlib.sha256(compiler.read_bytes()).hexdigest()}

    def run(stage,command,**kwargs):
        command=list(map(str,command));result=subprocess.run(command,capture_output=True,text=True,cwd=out,**kwargs)
        (out/(stage+".stdout")).write_text(result.stdout);(out/(stage+".stderr")).write_text(result.stderr)
        report["commands"].append({"stage":stage,"command":command,"exit":result.returncode})
        (out/"results.json").write_text(json.dumps(report,indent=2)+"\n")
        if result.returncode:raise RuntimeError(stage+" failed; see "+str(out))
        return result.stdout

    run("native-build",["g++","-std=c++17","-pthread",source,"-o",out/"native"])
    expected=run("native-run",[out/"native"])
    # Keep the libc++ search path local to its C++ inputs. Passing it through
    # dotnet.py would also make the separately added musl TUs include libc++'s
    # math.h/stdlib.h wrappers instead of the portable C provider headers.
    includes=provider["includeDirectories"]+[str(root/"edgcpp/backend/runtime/posix/include"),
                                             str(root/"edgcpp/backend/runtime/libc/include")]
    environment=dict(os.environ,EDG_BASE=str(root/"edgcpp/upstream/bases/docker/dev-env/gcc"),
                     EDG_DOT_TARGET="portable-lp64-binary80-v2")
    preprocessed=[]
    for index,unit in enumerate(units):
        command=[compiler,"--c++17","--gnu_version","130300","--clear_flag","use_predefined_macro_file",
                 "--preprocess",*[v for path in includes for v in ("-I",path)],unit]
        content=run("preprocess-"+str(index),command,env=environment)
        path=out/("input-"+str(index)+".ii");path.write_text(content)
        preprocessed.append(path)
    report["preprocessedInputs"]={str(path):hashlib.sha256(path.read_bytes()).hexdigest() for path in preprocessed}
    generated=out/"generated"
    command=[sys.executable,root/"edgcpp/dotnet.py",*preprocessed,"--compiler",compiler,"--std","c++17",
             "--libc","musl","--posix","--emit","exe","--output-dir",generated]
    run("translate",command)
    manifest=json.loads((generated/"dot-gen.json").read_text())
    if manifest.get("unboundExternals",{}).get("requiredPartials"):raise RuntimeError("Unresolved externs remain")
    report["checks"].append("Real LLVM18 provider + separate musl TUs translated with strict extern rejection; " +
                           ("separate C++ consumer with two fixture-only source-retention anchors" if args.separate_consumer else
                            "libc++ and C++ consumer share primary TU"))
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
