"""Reject malformed private synchronous signal fatal/diagnostic contracts."""
import argparse,json,subprocess,sys
from pathlib import Path


def verify_libc_signals_abi(root, tests, out):
    out.mkdir(parents=True,exist_ok=True)
    cases=[
      ('fatal-result','int __edg_signal_terminate(int);','__edg_signal_terminate','libc import ABI'),
      ('fatal-width','void __edg_signal_terminate(long);','__edg_signal_terminate','libc import ABI'),
      ('fatal-arity','void __edg_signal_terminate(int,int);','__edg_signal_terminate','libc import ABI'),
      ('fatal-variadic','void __edg_signal_terminate(int,...);','__edg_signal_terminate','libc import ABI'),
      ('report-result','int __edg_assert_report(const char*,const char*,int,const char*);','__edg_assert_report','libc import ABI'),
      ('report-pointee','void __edg_assert_report(void*,const char*,int,const char*);','__edg_assert_report','libc import ABI'),
      ('report-line-width','void __edg_assert_report(const char*,const char*,long,const char*);','__edg_assert_report','libc import ABI'),
      ('report-variadic','void __edg_assert_report(const char*,const char*,int,const char*,...);','__edg_assert_report','libc import ABI'),
    ]
    checks=[]
    for name,declaration,symbol,diagnostic in cases:
        source=out/f'{name}.c';source.write_text(declaration+f'\nvoid *address(void){{return (void*){symbol};}}\n')
        command=[sys.executable,root/'dotnet.py',source,'--language','c','--libc','dotcc','--target','portable-lp64-binary80-v1','--emit','managedlib','--output-dir',out/name]
        result=subprocess.run(list(map(str,command)),text=True,capture_output=True)
        (out/f'{name}.command.json').write_text(json.dumps(list(map(str,command)),indent=2));(out/f'{name}.log').write_text(result.stdout+result.stderr)
        assert result.returncode!=0,(name,result.stdout)
        assert diagnostic in result.stderr and symbol in result.stderr,(name,result.stderr)
        assert not (out/name/'Translated.cs').exists(),name
        checks.append(f'{name}: rejected before C# emission')
    return checks


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output-dir',type=Path,required=True);options=parser.parse_args()
    out=options.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[]
    try:checks=verify_libc_signals_abi(root,root/'backend/tests',out)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
