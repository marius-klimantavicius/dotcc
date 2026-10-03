"""Math imports reject incompatible declarations and unknown builtin-like names."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_math_abi(root,tests,out):
    out.mkdir(parents=True,exist_ok=True);checks=[]
    for name,declaration,symbol,message in [
        ('abs-result','long abs(int);','abs','libc import ABI'),
        ('sqrtl-argument','long double sqrtl(double);','sqrtl','libc import ABI'),
        ('copysignl-width','long double copysignl(long double,double);','copysignl','libc import ABI'),
        ('unknown-builtin','double __builtin_edg_math_private(double);','__builtin_edg_math_private','external function'),
        ('round-result','float round(double);','round','libc import ABI'),
        ('roundf-argument','float roundf(double);','roundf','libc import ABI'),
        ('public-rounding','double nearbyint(double);','nearbyint','external function'),
    ]:
        source=out/f'{name}.c';source.write_text(declaration+f'\nvoid *address(void){{return (void*){symbol};}}\n')
        command=list(map(str,[sys.executable,root/'dotnet.py',source,'--language','c','--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--output-dir',out/name]))
        result=subprocess.run(command,text=True,capture_output=True)
        (out/f'{name}.command.json').write_text(json.dumps(command,indent=2));(out/f'{name}.log').write_text(result.stdout+result.stderr)
        assert result.returncode!=0 and message in result.stderr and symbol in result.stderr,(name,result.stderr)
        assert not (out/name/'Translated.cs').exists(),name
        checks.append(f'{name}: rejected before C# emission')
    return checks

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    root=Path(__file__).resolve().parents[2];checks=[]
    try:checks=verify_libc_math_abi(root,root/'backend/tests',out)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
