"""Reject malformed text/float/FILE-lock contracts before C# emission."""
import argparse,json,subprocess,sys
from pathlib import Path


def verify_libc_import_abi(root, tests, out):
    out.mkdir(parents=True,exist_ok=True)
    prefix='typedef struct __edg_FILE FILE;typedef __builtin_va_list va_list;\n'
    callback='typedef int (*body)(void*,const char*,va_list);\n'
    cases=[
      ('callback-result',prefix+'typedef long (*body)(void*,const char*,va_list);\nint __edg_with_file_lock(FILE*,body,void*,const char*,va_list);','__edg_with_file_lock','libc import ABI'),
      ('callback-arguments',prefix+'typedef int (*body)(void*,const char*,va_list,int);\nint __edg_with_file_lock(FILE*,body,void*,const char*,va_list);','__edg_with_file_lock','libc import ABI'),
      ('fake-va-list',prefix+'struct fake {void *data[3];};typedef int (*body)(void*,const char*,struct fake*);\nint __edg_with_file_lock(FILE*,body,void*,const char*,struct fake*);','__edg_with_file_lock','libc import ABI'),
      ('va-array-pointer',prefix+callback+'int __edg_with_file_lock(FILE*,body,void*,const char*,va_list*);','__edg_with_file_lock','libc import ABI'),
      ('complete-file',prefix+'struct __edg_FILE {char byte;};'+callback+'int __edg_with_file_lock(FILE*,body,void*,const char*,va_list);','__edg_with_file_lock','libc import ABI'),
      ('ellipsis',prefix+callback+'int __edg_with_file_lock(FILE*,body,void*,const char*,va_list,...);','__edg_with_file_lock','libc import ABI'),
      ('error-pointer','void *strerror(int);','strerror','libc import ABI'),
      ('wide-width','int wctomb(char*,unsigned short);','wctomb','libc import ABI'),
      ('long-double-result','double frexpl(long double,int*);','frexpl','libc import ABI'),
      ('classifier-record','struct fake {unsigned long value;unsigned short exp;};int __edg_isfinitel(struct fake);','__edg_isfinitel','libc import ABI'),
      ('public-scalbn','double scalbn(double,int);','scalbn','external function'),
    ]
    cases += [('read-'+name,declaration.replace('__edg_with_file_lock','__edg_with_file_read_lock'),'__edg_with_file_read_lock',diagnostic) for name,declaration,symbol,diagnostic in cases[:6]]
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
    try:checks=verify_libc_import_abi(root,root/'backend/tests',out)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
