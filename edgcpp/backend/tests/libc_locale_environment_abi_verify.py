"""Reject malformed owner-environment locale bridge contracts."""
import argparse,json,subprocess,sys
from pathlib import Path


def verify_libc_locale_environment_abi(root, tests, out):
    out.mkdir(parents=True,exist_ok=True)
    cases=[
      ('set-name-result','int __edg_locale_set_name(int,const char*);','__edg_locale_set_name','libc import ABI'),
      ('set-name-category','char* __edg_locale_set_name(long,const char*);','__edg_locale_set_name','libc import ABI'),
      ('new-name-result','void* __edg_locale_new_name(int,const char*,locale_t);','__edg_locale_new_name','libc import ABI'),
      ('new-name-base','locale_t __edg_locale_new_name(int,const char*,void*);','__edg_locale_new_name','libc import ABI'),
      ('set-categories-result','void* __edg_locale_set_categories(int,int);','__edg_locale_set_categories','libc import ABI'),
      ('set-categories-width','char* __edg_locale_set_categories(int,long);','__edg_locale_set_categories','libc import ABI'),
      ('new-categories-base','locale_t __edg_locale_new_categories(int,int,void*);','__edg_locale_new_categories','libc import ABI'),
      ('new-categories-variadic','locale_t __edg_locale_new_categories(int,int,locale_t,...);','__edg_locale_new_categories','libc import ABI'),
    ]
    checks=[]
    for name,declaration,symbol,diagnostic in cases:
        source=out/f'{name}.c';source.write_text('#include <locale.h>\n'+declaration+f'\nvoid *address(void){{return (void*){symbol};}}\n')
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
    try:checks=verify_libc_locale_environment_abi(root,root/'backend/tests',out)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
