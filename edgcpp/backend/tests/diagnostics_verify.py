"""Backend rejections retain source/include/#line locations without output."""
import json,sys


def verify_diagnostics(root,tests,out,run):
    checks=[]
    cases=[('direct','extern int missing();\nint main(){\n return missing();\n}\n','direct.cpp:3:',None),
           ('logical','#line 321 "logical-input.cpp"\nextern int missing();\nint main(){return missing();}\n','logical-input.cpp:322:',None),
           ('included','#include "failure.h"\nint main(){return fail();}\n','failure.h:2:','extern int missing();\ninline int fail(){return missing();}\n'),
           ('local','int main(){\n alignas(8192) int value=2;\n return value;\n}\n','local.cpp:2:',None)]
    for name,source,position,header in cases:
        path=out/(name+'.cpp');path.write_text(source)
        if header:(out/'failure.h').write_text(header)
        result=run([sys.executable,root/'dotnet.py',path,'--output-dir',out/name],expected=1)
        assert position in result.stderr,(position,result.stderr)
        assert 'dot_gen_be: unsupported' in result.stderr,result.stderr
        assert not (out/name).exists()
        checks.append('source diagnostics: '+name+' location and atomic rejection')
    return checks
