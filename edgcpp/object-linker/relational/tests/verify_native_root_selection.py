#!/usr/bin/env python3
"""Native roots keep referenced weak bodies; unused captured bodies remain browsable."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def facts(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro',uri=True) as db:
        return db.execute('''SELECT s.name,s.is_definition,s.is_emission_root,s.link_name
            FROM symbols s JOIN functions f ON f.symbol_id=s.id ORDER BY s.id''').fetchall()


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,required=True)
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/native_root_selection',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
        objects.append(obj)
    provider,consumer=map(facts,objects)
    for rows in (provider,consumer):
        assert any(name=='unused_body' and defined and root==0 for name,defined,root,_ in rows),rows
    for name in ('called_helper','address_helper','Guard','~Guard'):
        assert any(n==name and defined and root==1 for n,defined,root,_ in provider),provider
        assert any(n==name and defined and root==0 for n,defined,root,_ in consumer),consumer
    (work/'database-summary.txt').write_text('provider: '+str(provider)+'\nmain: '+str(consumer)+'\n')
    source.rename(work/'original-source')

    def link(label,inputs,options=(),unused=False):
        output=work/label
        run(['dotnet',args.linker.resolve(),'link','--out',output,'--entry','main',
             '--namespace','Review.Selection','--class','Core',*options,*inputs],work/(label+'-link.log'))
        code=(output/'Translated.cs').read_text()
        assert ('unused_body(' in code)==unused,code
        assert 'called_helper(' in code and 'address_helper(' in code and 'provider(' in code
        assert 'Guard' in code and 'for (i = 0;' in code,code
        log=(work/(label+'-link.log')).read_text()
        assert 'peak bodies 1' in log,log
        run(['dotnet','run','--project',output/'Translated.csproj','-c','Release'],work/(label+'-managed.log'))
        return output

    # main first selects its weak definitions, whose own root facts are false.
    # provider's actual references/root facts must retain those winning bodies.
    for label,order in (('main-first',objects[::-1]),('provider-first',objects)):
        generated=link(label,order)
        if args.nativeaot:
            publish=work/(label+'-nativeaot')
            run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true','-o',publish],work/(label+'-nativeaot-build.log'))
            run([publish/'Translated'],work/(label+'-nativeaot.log'))
    link('all-bodies',objects,['--emission-selection','all-bodies'],unused=True)

    # Controlled root-annotation variants retain the same EDG executable graph.
    # They exercise declaration-to-definition transfer and conservative unknown
    # handling separately from the genuine native root facts above.
    declaration_objects=[]
    for original in objects:
        copy=work/('declaration-'+original.name);shutil.copy2(original,copy)
        with sqlite3.connect(copy) as db:
            db.execute("UPDATE symbols SET is_emission_root=0 WHERE name='provider'")
            if original.stem=='main':
                db.execute("UPDATE symbols SET is_emission_root=1 WHERE name='provider' AND is_definition=0")
        declaration_objects.append(copy)
    link('declaration-root',declaration_objects[::-1])

    null_object=work/'unknown-main.dotobj';shutil.copy2(objects[1],null_object)
    with sqlite3.connect(null_object) as db:
        db.execute("UPDATE symbols SET is_emission_root=NULL WHERE name='unused_body'")
    link('unknown-root',[null_object,objects[0]],unused=True)
    assert 'conservative unknown roots 1' in (work/'unknown-root-link.log').read_text()

    legacy=[]
    for original in objects:
        copy=work/('legacy-'+original.name);shutil.copy2(original,copy)
        with sqlite3.connect(copy) as db:db.execute('ALTER TABLE symbols DROP COLUMN is_emission_root')
        legacy.append(copy)
    link('legacy',legacy,unused=True)
    assert 'legacy objects without root facts' in (work/'legacy-link.log').read_text()
    assert facts(objects[0])==provider and facts(objects[1])==consumer
    print('PASS: source-hidden native/C#'+('/NativeAOT' if args.nativeaot else '')+
          ' roots/reference/initializer/cleanup closure; nonroot weak winners in both orders; '
          'unused body retained in SQL; explicit all-bodies, declaration roots, NULL and legacy fallback; discarded for assignment')
    print(work)


if __name__=='__main__':main()
