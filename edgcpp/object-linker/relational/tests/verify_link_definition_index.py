#!/usr/bin/env python3
"""Genuine weak/strong providers preserve ordinary disk-backed link selection."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys
from verify import RELATIONAL, run


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,required=True)
    parser.add_argument('--linker',type=Path,required=True)
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/link_definition_index',source)
    run(['g++','-std=c++17',source/'weak.cpp',source/'strong.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects={}
    for name in ('weak','strong','main','duplicate'):
        obj=work/(name+'.dotobj');objects[name]=obj
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--std','c++17',
             '--output',obj,'--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
    for name,kind in (('weak','weak'),('strong','strong'),('duplicate','strong')):
        with sqlite3.connect(objects[name]) as db:
            assert db.execute("SELECT definition_kind FROM symbols WHERE name='shared_value'").fetchone()==(kind,)
    source.rename(work/'hidden-source')
    for label,names in (('weak-first',('weak','strong','main')),('strong-first',('strong','weak','main'))):
        output=work/label
        run(['dotnet',args.linker.resolve(),'link','--out',output,'--entry','main',*[objects[name] for name in names]],work/(label+'-link.log'))
        code=(output/'Translated.cs').read_text()
        assert '*shared_value = 9;' in code and '*shared_value = 7;' not in code,code
        run(['dotnet','run','--project',output/'Translated.csproj','-c','Release'],work/(label+'-managed.log'))
        indices=list(output.glob('.link-index-*.sqlite'));assert len(indices)==1
        with sqlite3.connect(indices[0]) as db:
            winner=db.execute('SELECT i.path,d.definition_kind FROM external_definitions d JOIN inputs i USING(unit_id) WHERE d.link_name=?',('shared_value',)).fetchone()
            assert winner==(str(objects['strong']),'strong'),winner
    argv=['dotnet',str(args.linker.resolve()),'link','--out',str(work/'duplicate-strong'),'--entry','main',
          *[str(objects[name]) for name in ('strong','duplicate','main')]]
    result=subprocess.run(argv,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
    (work/'duplicate-strong.log').write_text(result.stdout)
    assert result.returncode!=0 and 'Multiple strong definitions of shared_value' in result.stdout,result.stdout
    assert not (work/'duplicate-strong/Translated.cs').exists()
    print('PASS: genuine weak/strong providers in both orders; exact SQLite winner; duplicate strong rejected; inputs source-hidden')
    print(work)


if __name__=='__main__':main()
