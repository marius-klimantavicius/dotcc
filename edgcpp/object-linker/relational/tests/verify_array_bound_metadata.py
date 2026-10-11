#!/usr/bin/env python3
"""Source array bound recipes with separately selected concrete execution."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro',uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        assert db.execute('PRAGMA user_version').fetchone()[0]==1023
        arrays=db.execute("SELECT * FROM types WHERE kind='array' ORDER BY id").fetchall()
        dependent=[row for row in arrays if row['array_bound_kind']=='dependent']
        assert dependent, [dict(row) for row in arrays]
        bound_kinds=set(); bound_nodes=set()
        for row in dependent:
            assert row['element_count'] is None and row['is_dependent'], dict(row)
            if row['array_bound_node_id'] is None:
                continue  # EDG permits a genuinely unknown dependent extent.
            bound=db.execute('SELECT * FROM nodes WHERE id=?',(row['array_bound_node_id'],)).fetchone()
            assert bound['domain']=='template-recipe' and bound['function_id'] is None, dict(bound)
            pending=[bound['id']]
            while pending:
                node_id=pending.pop()
                if node_id in bound_nodes:
                    continue
                bound_nodes.add(node_id)
                selected=db.execute('SELECT * FROM nodes WHERE id=?',(node_id,)).fetchone()
                assert selected['domain']=='template-recipe' and selected['function_id'] is None, dict(selected)
                bound_kinds.add(selected['kind'])
                pending.extend(child[0] for child in db.execute(
                    "SELECT child_id FROM edges WHERE parent_id=? AND role='operand' ORDER BY ordinal",(node_id,)))
        assert 'dependent-expression' in bound_kinds and 'dependent-member-reference' in bound_kinds, bound_kinds
        packs=db.execute("SELECT * FROM nodes WHERE kind='sizeof-pack'").fetchall()
        assert packs and all(pack['id'] in bound_nodes for pack in packs)
        for pack in packs:
            assert pack['domain']=='template-recipe' and pack['function_id'] is None, dict(pack)
            assert pack['operator']=='type' and pack['constant_text'] is None, dict(pack)
            operand=db.execute('''SELECT n.*, p.is_pack, p.parameter_depth, p.parameter_position
                FROM edges e JOIN nodes n ON n.id=e.child_id
                JOIN template_type_parameters p ON p.type_id=n.type_id
                WHERE e.parent_id=? AND e.role='operand' AND e.ordinal=0''',(pack['id'],)).fetchone()
            assert operand and operand['kind']=='type-reference' and operand['is_pack'], dict(operand) if operand else None
            assert operand['source_node_id']==pack['id'] and operand['value_category'] is None, dict(operand)
            assert operand['parameter_depth'] is not None and operand['parameter_position'] is not None, dict(operand)
        def parameter_array(name):
            row=db.execute('''SELECT array.* FROM symbols s JOIN type_parameters p ON p.type_id=s.type_id AND p.ordinal=0
                JOIN types pointer ON pointer.id=p.parameter_type_id JOIN types array ON array.id=pointer.pointee_type_id
                WHERE s.name=?''',(name,)).fetchone()
            assert row and row['kind']=='array', name
            return row

        fixed=parameter_array('browse_fixed')
        zero=parameter_array('browse_zero')
        incomplete=parameter_array('browse_incomplete')
        assert fixed['element_count']==3 and not fixed['is_incomplete'], dict(fixed)
        assert zero['element_count']==0 and not zero['is_incomplete'], dict(zero)
        assert incomplete['element_count'] is None and incomplete['is_incomplete'], dict(incomplete)
        assert all(row['array_bound_kind'] is None and row['array_bound_node_id'] is None
                   for row in (fixed,zero,incomplete))
        assert not db.execute('''SELECT 1 FROM types t JOIN nodes n ON n.id=t.array_bound_node_id
            WHERE n.domain!='template-recipe' OR n.function_id IS NOT NULL''').fetchone()
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert len(tables)==44 and not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',
                          *(str(dict(row)) for row in arrays)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/array_bound_metadata.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'arrays.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--output',obj,'--work-dir',work/'compile-work',
         '--compiler',args.compiler.resolve()],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    source.rename(work/'original-source.cpp');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',obj],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    unsupported=work/'unsupported-vla.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',RELATIONAL/'tests/unsupported_vla_bound.cpp',
         '--output',unsupported,'--std','c11','--work-dir',work/'vla-work','--compiler',args.compiler.resolve()],
        work/'vla-compile.log',expected=1)
    diagnostic=(work/'vla-work/stderr.txt').read_text()
    assert not unsupported.exists() and 'VLA type requires selected dimension correspondence' in diagnostic, diagnostic
    assert 'vla=1' in diagnostic and 'local-dependent=' in diagnostic, diagnostic
    print('PASS: selected dependent array bound recipes, exact fixed/zero/incomplete extents, '
          'native/source-hidden concrete C#'+(', NativeAOT' if args.nativeaot else '')+'; explicit VLA dimension boundary')
    print(work)


if __name__=='__main__':main()
