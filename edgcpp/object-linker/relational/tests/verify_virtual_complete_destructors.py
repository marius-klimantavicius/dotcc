#!/usr/bin/env python3
"""Source/D1/D2 identity and exact guarded/copied virtual-base destruction."""
import argparse
import hashlib
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        assert db.execute('PRAGMA user_version').fetchone()[0]>=1029
        source=db.execute("SELECT * FROM symbols WHERE kind='source-function' AND name='~Leaf'").fetchone()
        assert source and source['is_definition'], dict(source) if source else None
        entries=db.execute('SELECT e.*,s.link_name,s.type_id,s.kind,s.is_definition '
            'FROM function_abi_entries e JOIN symbols s ON s.id=e.entry_symbol_id '
            'WHERE e.source_symbol_id=? ORDER BY e.variant',(source['id'],)).fetchall()
        by_variant={row['variant']:row for row in entries}
        assert {'base','complete'}<=set(by_variant), [dict(row) for row in entries]
        base,complete=by_variant['base'],by_variant['complete']
        assert base['entry_symbol_id']!=complete['entry_symbol_id']!=source['id']
        assert base['link_name'].endswith('D2Ev') and complete['link_name'].endswith('D1Ev')
        for entry,count in ((base,2),(complete,1)):
            assert entry['kind']=='function' and entry['is_definition']
            assert entry['type_id']==entry['callable_type_id'] and entry['type_id']!=source['type_id']
            signature=db.execute('SELECT * FROM types WHERE id=?',(entry['type_id'],)).fetchone()
            assert signature['parameters_include_receiver']==1
            assert db.execute('SELECT count(*) FROM type_parameters WHERE type_id=?',(entry['type_id'],)).fetchone()[0]==count
            assert db.execute('SELECT count(*) FROM parameters WHERE function_id=?',(entry['entry_symbol_id'],)).fetchone()[0]==count
        assert db.execute('SELECT count(*) FROM type_parameters WHERE type_id=?',(source['type_id'],)).fetchone()[0]==0
        logical=db.execute('SELECT * FROM functions WHERE symbol_id=?',(source['id'],)).fetchone()
        physical=db.execute('SELECT * FROM functions WHERE symbol_id=?',(base['entry_symbol_id'],)).fetchone()
        assert logical and physical and logical['body_node_id']==physical['body_node_id']
        root=db.execute('SELECT * FROM nodes WHERE id=?',(physical['body_node_id'],)).fetchone()
        assert root['function_id']==base['entry_symbol_id']
        assert not db.execute('SELECT 1 FROM nodes WHERE function_id=?',(source['id'],)).fetchone()
        copied=db.execute("SELECT c.*,i.source_initialization_id,i.lifetime_id,i.function_id AS action_owner,"
            "i.selected_cleanup_entry_symbol_id,i.selected_cleanup_callable_type_id,i.destruction_condition_node_id "
            "FROM constructor_initializers c "
            "JOIN initializations i ON i.id=c.initialization_id WHERE c.function_id=? AND c.kind='virtual-base'",
            (complete['entry_symbol_id'],)).fetchall()
        assert len(copied)==1, [dict(row) for row in copied]
        action=copied[0]
        assert action['action_owner']==complete['entry_symbol_id'] and action['source_initialization_id'] is not None
        original=db.execute('SELECT * FROM initializations WHERE id=?',(action['source_initialization_id'],)).fetchone()
        assert original and original['function_id']==base['entry_symbol_id'] and original['lifetime_id'] is not None
        assert action['destruction_condition_node_id'] is None
        condition=original['destruction_condition_node_id']
        assert condition is not None
        entry_conditions=db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='destruction-condition'",
            (physical['body_node_id'],)).fetchall()
        assert [row['child_id'] for row in entry_conditions]==[condition]
        comparison=db.execute('SELECT * FROM nodes WHERE id=?',(condition,)).fetchone()
        assert comparison['function_id']==base['entry_symbol_id'] and comparison['operator']=='eq'
        operands=db.execute('SELECT n.* FROM edges e JOIN nodes n ON n.id=e.child_id '
            "WHERE e.parent_id=? AND e.role='operand' ORDER BY ordinal",(condition,)).fetchall()
        assert len(operands)==2 and operands[0]['kind']=='variable' and operands[1]['constant_text']=='0'
        vtt_parameter=db.execute('SELECT symbol_id FROM parameters WHERE function_id=? AND ordinal=1',
            (base['entry_symbol_id'],)).fetchone()[0]
        assert operands[0]['symbol_id']==vtt_parameter
        retained=db.execute('SELECT d.* FROM lifetime_destructions d JOIN lifetimes l ON l.id=d.lifetime_id '
            'WHERE l.function_id=? AND d.initialization_id=?',(base['entry_symbol_id'],original['id'])).fetchall()
        assert len(retained)==1
        assert db.execute("SELECT 1 FROM constructor_initializers WHERE function_id=? AND kind='virtual-base' AND initialization_id=?",
            (base['entry_symbol_id'],original['id'])).fetchone()
        prologue=db.execute("SELECT n.* FROM edges e JOIN nodes n ON n.id=e.child_id "
            "WHERE e.parent_id=? AND e.role='abi-prologue'",(physical['body_node_id'],)).fetchall()
        assert len(prologue)==1 and prologue[0]['kind']=='if'
        assert db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='condition'",
            (prologue[0]['id'],)).fetchone()[0]==condition
        assignment=db.execute("SELECT n.* FROM edges t JOIN edges v ON v.parent_id=t.child_id "
            "AND v.role='value' JOIN nodes n ON n.id=v.child_id WHERE t.parent_id=? AND t.role='then'",
            (prologue[0]['id'],)).fetchone()
        assert assignment and assignment['operator']=='assign'
        destination=db.execute("SELECT n.* FROM edges e JOIN nodes n ON n.id=e.child_id "
            "WHERE e.parent_id=? AND e.role='operand' AND e.ordinal=0",(assignment['id'],)).fetchone()
        assert destination['kind']=='variable' and destination['symbol_id']==vtt_parameter
        destruction=db.execute('SELECT d.*,l.function_id FROM lifetime_destructions d '
            'JOIN lifetimes l ON l.id=d.lifetime_id WHERE d.initialization_id=?',(action['initialization_id'],)).fetchall()
        assert len(destruction)==1 and destruction[0]['function_id']==complete['entry_symbol_id']
        assert action['selected_cleanup_entry_symbol_id'] is not None
        selected=db.execute('SELECT * FROM symbols WHERE id=?',(action['selected_cleanup_entry_symbol_id'],)).fetchone()
        assert selected['link_name'].endswith('SharedD2Ev')
        alias=db.execute('SELECT e.*,s.link_name,s.kind FROM function_abi_entries e '
            'JOIN symbols s ON s.id=e.source_symbol_id WHERE e.entry_symbol_id=?',(selected['id'],)).fetchone()
        assert alias and alias['alias_target_symbol_id']==alias['source_symbol_id']
        assert alias['link_name'].endswith('SharedD1Ev') and alias['kind']=='function'
        calls=db.execute("SELECT n.*,s.link_name FROM nodes n JOIN symbols s ON s.id=n.symbol_id "
            "WHERE n.function_id=? AND n.kind='function'",(complete['entry_symbol_id'],)).fetchall()
        assert any(row['symbol_id']==base['entry_symbol_id'] and row['type_id']==base['callable_type_id'] for row in calls)
        vtts=db.execute("SELECT s.*,t.element_count FROM symbols s JOIN types t ON t.id=s.type_id "
            "WHERE s.kind='virtual-table-table'").fetchall()
        assert vtts
        for vtt in vtts:
            assert not db.execute('SELECT 1 FROM global_initializers WHERE symbol_id=?',(vtt['id'],)).fetchone()
            values=db.execute('SELECT e.*,t.address_point_byte_offset AS selected_address FROM vtt_entries e '
                'JOIN virtual_tables t ON t.id=e.virtual_table_id WHERE symbol_id=? ORDER BY ordinal',(vtt['id'],)).fetchall()
            assert values and [row['ordinal'] for row in values]==list(range(len(values)))
            assert all(row['address_point_byte_offset']==row['selected_address'] for row in values)
            if vtt['is_definition']:
                assert vtt['element_count']==len(values)
        assert db.execute("SELECT 1 FROM nodes n JOIN symbols s ON s.id=n.symbol_id "
            "WHERE n.function_id=? AND n.kind IN('vtt-address','variable') AND s.kind='virtual-table-table'",
            (complete['entry_symbol_id'],)).fetchone()
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',str(dict(source)),
            *(str(dict(row)) for row in entries),str(dict(action)),*(str(dict(row)) for row in vtts)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/virtual_complete_destructors.cpp',source)
    compiler=args.compiler.resolve()
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p) for p in
        (compiler,source,RELATIONAL/'compile.py',Path(__file__).resolve()))+'\n')
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'virtual-complete-destructors.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--std','c++17','--output',obj,'--work-dir',work/'compile-work',
         '--compiler',compiler],work/'compile.log')
    source.rename(work/'hidden-source.cpp')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    print('PASS: native destruction once and source-hidden exact D1/D2/body/entry-guard/copy/VTT SQL correspondence')
    print(work)


if __name__=='__main__':main()
