"""Reject fake locale and incompatible lconv layouts before managed emission."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_locale_abi(root,tests,out):
    out.mkdir(parents=True,exist_ok=True);header=(root/'backend/runtime/libc/include/locale.h').read_text();layout=header[header.index('struct lconv {'):header.index('struct lconv *localeconv')]
    cases=[
      ('complete-locale','struct __edg_locale {int value;};typedef struct __edg_locale *locale_t;locale_t duplocale(locale_t);','duplocale'),
      ('wrong-locale-tag','typedef struct other *locale_t;locale_t uselocale(locale_t);','uselocale'),
      ('wrong-mask','typedef struct __edg_locale *locale_t;locale_t newlocale(long,const char*,locale_t);','newlocale'),
      ('wrong-lconv-layout','struct lconv {char *fields[10];char flags[14];};struct lconv *localeconv(void);','localeconv'),
      ('wrong-lconv-signedness',layout.replace('char int_frac_digits;','unsigned char int_frac_digits;')+'struct lconv *localeconv(void);','localeconv'),
      ('fake-locale-validator','int __edg_locale_validate(void*);','__edg_locale_validate'),
      ('gnu-strerror-r','char *strerror_r(int,char*,unsigned long);','strerror_r')]
    checks=[]
    for name,source,symbol in cases:
        path=out/f'{name}.c';path.write_text(source+f'\nvoid *address(void){{return (void*){symbol};}}\n')
        command=list(map(str,[sys.executable,root/'dotnet.py',path,'--language','c','--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--output-dir',out/name]))
        r=subprocess.run(command,capture_output=True,text=True);(out/f'{name}.command.json').write_text(json.dumps(command,indent=2));(out/f'{name}.log').write_text(r.stdout+r.stderr)
        assert r.returncode!=0 and 'libc import ABI' in r.stderr and symbol in r.stderr,(name,r.stderr)
        assert not (out/name/'Translated.cs').exists();checks.append(f'{name}: rejected before C# emission')
    return checks

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',required=True,type=Path);a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[]
    try:checks=verify_libc_locale_abi(root,root/'backend/tests',out)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
