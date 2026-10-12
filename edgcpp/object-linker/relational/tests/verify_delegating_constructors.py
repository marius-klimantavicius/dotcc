#!/usr/bin/env python3
"""Recorded cleanup-free complete-object delegation preserves the receiver."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,required=True)
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/delegating_constructors',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
        objects.append(obj)
    with sqlite3.connect(objects[0]) as db:
        rows=db.execute('''SELECT c.function_id,c.target_type_id,c.is_virtual,i.kind,i.type_id,
            i.constructor_symbol_id,i.destructor_symbol_id,i.lifetime_id,i.init_expr_lifetime_id
            FROM constructor_initializers c JOIN initializations i ON i.id=c.initialization_id
            WHERE c.kind='delegation' ORDER BY c.function_id''').fetchall()
        assert len(rows)==2 and all(not virtual and kind=='constructor' and target==type_id and constructor
            and destructor is None and lifetime is None and expression is None
            for _,target,virtual,kind,type_id,constructor,destructor,lifetime,expression in rows),rows
        (work/'database-summary.txt').write_text(str(rows)+'\n')
    source.rename(work/'original-source')
    for label,order in (('provider-first',objects),('main-first',objects[::-1])):
        generated=work/label
        run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*order],work/(label+'-link.log'))
        code=(generated/'Translated.cs').read_text()
        assert '(&(*' in code and 'selected_argument(' in code,code
        run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/(label+'-managed.log'))
        if args.nativeaot:
            output=work/(label+'-nativeaot')
            run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true','-o',output],work/(label+'-nativeaot-build.log'))
            run([output/'Translated'],work/(label+'-nativeaot.log'))
    print('PASS: selected cleanup-free delegation into the same complete receiver; nested calls, argument effects once, '
          'argument/target/body failure; native/source-hidden C#'+('/NativeAOT' if args.nativeaot else '')+' in both link orders')
    print(work)


if __name__=='__main__':main()
