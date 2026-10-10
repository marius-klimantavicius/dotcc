#!/usr/bin/env python3
"""Verify owned template argument identities and symbolic original array bounds."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--case', choices=('arguments', 'bounds', 'members', 'traits', 'member-templates'), required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / (args.case + '.cpp')
    shutil.copy2(Path(__file__).with_name({'arguments': 'duckdb_source_record_template_arguments.cpp', 'bounds': 'duckdb_source_dependent_array_bounds.cpp', 'members': 'duckdb_source_dependent_member_constants.cpp', 'traits': 'duckdb_source_typed_traits.cpp', 'member-templates': 'duckdb_source_member_templates.cpp'}[args.case]), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)

    error = None
    try:
        for optimization in ('0', '2'):
            run('native-build-' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, '-o', out / ('native-' + optimization)])
            run('native-run-' + optimization, [out / ('native-' + optimization)])
        obj = out / 'unit.dotobj'
        run('export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj, '--work-dir', out / 'work',
            '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            if args.case == 'arguments':
                check('owned-dependent-instantiations', db.execute('SELECT count(*) FROM source_record_instantiations').fetchone()[0] >= 3)
                check('nested-source-type-arguments', db.execute("SELECT count(*) FROM source_template_arguments a JOIN source_types t ON t.id=a.type_id WHERE a.kind='type' AND t.kind='record'").fetchone()[0] >= 2)
                check('qualifier-distinguishes-arguments', db.execute("SELECT count(*) FROM source_template_arguments a JOIN source_types t ON t.id=a.type_id WHERE t.is_const=1").fetchone()[0] > 0)
                check('distinct-dependent-record-identities', db.execute("SELECT count(DISTINCT identity_key) FROM source_record_types WHERE source_name='DuckdbSourceTraits' AND is_dependent=1 AND is_prototype_instantiation=0").fetchone()[0] >= 2)
                check('distinct-parameter-binders', db.execute("SELECT count(DISTINCT d.identity_key) FROM source_template_arguments a JOIN source_dependent_types d ON d.type_id=a.type_id JOIN source_types t ON t.id=d.type_id WHERE d.kind='parameter' AND t.source_name='T'").fetchone()[0] >= 2)
            elif args.case == 'member-templates':
                check('owned-member-template-lookup', db.execute("SELECT count(*) FROM source_record_instantiations WHERE resolution_kind='dependent-member-template'").fetchone()[0] >= 4)
                check('no-fabricated-member-prototype', db.execute("SELECT count(*) FROM source_record_instantiations WHERE resolution_kind='dependent-member-template' AND prototype_source_record_id IS NOT NULL").fetchone()[0] == 0)
                check('member-owner-source-type-shapes', db.execute("SELECT count(DISTINCT t.kind) FROM source_record_instantiations i JOIN source_types t ON t.id=i.member_owner_source_type_id WHERE i.resolution_kind='dependent-member-template'").fetchone()[0] >= 2)
                check('ordered-member-template-arguments', db.execute("SELECT count(*) FROM source_record_instantiations i WHERE resolution_kind='dependent-member-template' AND (SELECT count(*) FROM source_template_arguments a WHERE a.source_record_id=i.source_record_id)=2").fetchone()[0] >= 3)
                check('member-owner-and-argument-identities', db.execute("SELECT count(DISTINCT r.identity_key) FROM source_record_instantiations i JOIN source_record_types r ON r.id=i.source_record_id WHERE r.source_name='apply' AND i.resolution_kind='dependent-member-template'").fetchone()[0] >= 3)
                check('source-only-member-template', db.execute("SELECT count(*) FROM source_record_instantiations i JOIN source_record_types r ON r.id=i.source_record_id JOIN source_types t ON t.id=r.source_type_id WHERE i.resolution_kind='dependent-member-template' AND t.execution_type_id IS NOT NULL").fetchone()[0] == 0)
            elif args.case == 'traits':
                check('typed-trait-opcodes', db.execute("SELECT count(DISTINCT builtin_operator_kind) FROM source_semantic_constants WHERE kind='builtin-trait'").fetchone()[0] >= 3)
                check('exact-type-operands', db.execute("SELECT count(*) FROM source_semantic_constants WHERE kind='type-operand' AND operand_source_type_id IS NOT NULL").fetchone()[0] >= 7)
                check('variadic-type-trait', db.execute("SELECT count(*) FROM source_semantic_constants c WHERE c.kind='builtin-trait' AND c.operator_name='__is_constructible' AND (SELECT count(*) FROM source_semantic_constant_operands e WHERE e.constant_id=c.id)=3").fetchone()[0] >= 1)
                check('both-alignof-forms', db.execute("SELECT count(DISTINCT is_standard_alignof) FROM source_semantic_constants WHERE kind='alignof'").fetchone()[0] == 2)
                check('type-and-expression-sizeof', db.execute("SELECT count(DISTINCT operand_source_type_id IS NULL) FROM source_semantic_constants WHERE kind='sizeof'").fetchone()[0] == 2)
                check('dependent-type-query-bound', db.execute("WITH RECURSIVE bound_nodes(id) AS (SELECT constant_id FROM source_array_bounds UNION SELECT e.operand_id FROM source_semantic_constant_operands e JOIN bound_nodes b ON b.id=e.constant_id) SELECT count(*) FROM bound_nodes b JOIN source_semantic_constants c ON c.id=b.id WHERE c.kind IN ('sizeof','alignof')").fetchone()[0] >= 2)
                check('ordered-type-trait-identities', db.execute("SELECT count(DISTINCT identity_key) FROM source_record_types WHERE source_name='DuckdbSourceTraitResult' AND is_dependent=1 AND is_prototype_instantiation=0").fetchone()[0] >= 5)
            elif args.case == 'members':
                check('owned-dependent-member-constants', db.execute("SELECT count(*) FROM source_semantic_constants WHERE kind='member' AND owner_source_type_id IS NOT NULL").fetchone()[0] >= 3)
                check('member-owner-type-retained', db.execute("SELECT count(DISTINCT owner_source_type_id) FROM source_semantic_constants WHERE kind='member' AND source_name='value'").fetchone()[0] >= 2)
                check('member-distinguishes-template-identity', db.execute("SELECT count(DISTINCT r.identity_key) FROM source_record_instantiations i JOIN source_record_types r ON r.id=i.source_record_id WHERE r.source_name='DuckdbSourceTag'").fetchone()[0] >= 2)
                check('unresolved-member-type-retained', db.execute("SELECT count(*) FROM source_semantic_constants c JOIN source_types t ON t.id=c.source_type_id WHERE c.kind='member' AND t.kind='dependent' AND t.execution_type_id IS NULL").fetchone()[0] > 0)
                check('dependent-member-array-bound', db.execute("WITH RECURSIVE bound_nodes(id) AS (SELECT constant_id FROM source_array_bounds UNION SELECT e.operand_id FROM source_semantic_constant_operands e JOIN bound_nodes b ON b.id=e.constant_id) SELECT count(*) FROM bound_nodes b JOIN source_semantic_constants c ON c.id=b.id WHERE c.kind='member' AND c.source_name='extent'").fetchone()[0] >= 1)
            else:
                check('dependent-array-bound-marker', db.execute("SELECT count(*) FROM source_types WHERE kind='array' AND array_bound_kind='dependent'").fetchone()[0] >= 2)
                check('unsized-array-distinct', db.execute("SELECT count(*) FROM source_types WHERE kind='array' AND array_bound_kind='unspecified'").fetchone()[0] >= 1)
                check('typed-bound-parameter', db.execute("SELECT count(*) FROM source_semantic_constants WHERE kind='parameter' AND source_name='Size' AND parameter_depth>0 AND parameter_position>0").fetchone()[0] >= 1)
                check('owned-bound-addition', db.execute("SELECT count(*) FROM source_semantic_constants WHERE kind='operation' AND operator_name='add'").fetchone()[0] >= 1)
                check('nested-dependent-array', db.execute("SELECT count(*) FROM source_types a JOIN source_types e ON e.id=a.underlying_type_id WHERE a.kind='array' AND a.element_count=2 AND e.array_bound_kind='dependent'").fetchone()[0] >= 1)
                check('no-fabricated-dependent-layout', db.execute("SELECT count(*) FROM source_types WHERE array_bound_kind='dependent' AND execution_type_id IS NOT NULL").fetchone()[0] == 0)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', obj])
        run('build', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('jit', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '-v:q'])
            run('nativeaot', [out / 'aot/Translated'])
        negatives = (
            [('reject-argument-substitution', "UPDATE source_template_arguments SET type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE kind='type' AND rowid=(SELECT min(rowid) FROM source_template_arguments WHERE kind='type')"),
             ('reject-argument-omission', 'DELETE FROM source_template_arguments WHERE rowid=(SELECT min(rowid) FROM source_template_arguments)'),
             ('reject-prototype-mapping', "UPDATE source_record_instantiations SET prototype_source_record_id=(SELECT id FROM source_record_types WHERE source_name='DuckdbSourceTemplateOwner' AND is_prototype_instantiation=1 LIMIT 1) WHERE rowid=(SELECT min(rowid) FROM source_record_instantiations)")]
            if args.case == 'arguments' else
            [('reject-missing-bound', 'DELETE FROM source_array_bounds WHERE type_id=(SELECT min(type_id) FROM source_array_bounds)'),
             ('reject-erased-bound-kind', "UPDATE source_types SET array_bound_kind='unspecified' WHERE array_bound_kind='dependent'"),
             ('reject-bound-operand-omission', 'DELETE FROM source_semantic_constant_operands WHERE rowid=(SELECT min(rowid) FROM source_semantic_constant_operands)'),
             ('reject-bound-cycle', 'UPDATE source_semantic_constant_operands SET operand_id=constant_id WHERE rowid=(SELECT min(rowid) FROM source_semantic_constant_operands)')])
        if args.case == 'members':
            negatives = [
                ('reject-member-owner-substitution', "UPDATE source_semantic_constants SET owner_source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE kind='member'"),
                ('reject-member-name-substitution', "UPDATE source_semantic_constants SET source_name=source_name||'_changed' WHERE kind='member' AND source_name='value'"),
                ('reject-member-owner-mapping', "UPDATE source_semantic_constants SET owner_source_type_id=(SELECT owner_source_type_id FROM source_semantic_constants WHERE kind='member' AND source_name='value' ORDER BY id LIMIT 1) WHERE kind='member' AND source_name='value'")]
        if args.case == 'traits':
            negatives = [
                ('reject-trait-opcode-name', "UPDATE source_semantic_constants SET builtin_operator_kind=17 WHERE kind='builtin-trait' AND operator_name='__is_empty'"),
                ('reject-trait-missing-type', "DELETE FROM source_semantic_constant_operands WHERE constant_id=(SELECT min(id) FROM source_semantic_constants WHERE operator_name='__is_base_of') AND ordinal=1"),
                ('reject-trait-reordered-types', "UPDATE source_semantic_constant_operands SET operand_id=CASE ordinal WHEN 0 THEN (SELECT operand_id FROM source_semantic_constant_operands WHERE constant_id=(SELECT min(id) FROM source_semantic_constants WHERE operator_name='__is_base_of') AND ordinal=1) ELSE (SELECT operand_id FROM source_semantic_constant_operands WHERE constant_id=(SELECT min(id) FROM source_semantic_constants WHERE operator_name='__is_base_of') AND ordinal=0) END WHERE constant_id=(SELECT min(id) FROM source_semantic_constants WHERE operator_name='__is_base_of')"),
                ('reject-trait-binder-substitution', "UPDATE source_semantic_constants SET operand_source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE kind='type-operand' AND id=(SELECT min(id) FROM source_semantic_constants WHERE kind='type-operand')"),
                ('reject-sizeof-type-omission', "UPDATE source_semantic_constants SET operand_source_type_id=NULL WHERE kind='sizeof' AND operand_source_type_id IS NOT NULL"),
                ('reject-standard-alignof-erasure', "UPDATE source_semantic_constants SET is_standard_alignof=0 WHERE kind='alignof' AND is_standard_alignof=1")]
        if args.case == 'member-templates':
            negatives = [
                ('reject-member-template-owner-type', "UPDATE source_record_instantiations SET member_owner_source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE resolution_kind='dependent-member-template'"),
                ('reject-member-template-owner-substitution', "UPDATE source_record_instantiations SET member_owner_source_type_id=(SELECT min(member_owner_source_type_id) FROM source_record_instantiations WHERE resolution_kind='dependent-member-template') WHERE resolution_kind='dependent-member-template'"),
                ('reject-member-template-argument-omission', "DELETE FROM source_template_arguments WHERE source_record_id=(SELECT min(source_record_id) FROM source_record_instantiations WHERE resolution_kind='dependent-member-template')"),
                ('reject-member-template-fabricated-prototype', "UPDATE source_record_instantiations SET resolution_kind='class-template',prototype_source_record_id=(SELECT min(prototype_source_record_id) FROM source_record_instantiations WHERE resolution_kind='class-template'),member_owner_source_type_id=NULL,member_of_unknown_base=0,qualified_unknown_base_member=0,capture_proof='original-template-arguments' WHERE resolution_kind='dependent-member-template'")]
        for name, sql in negatives:
            damaged = out / (name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                if name == 'reject-trait-reordered-types':
                    owner = db.execute("SELECT min(id) FROM source_semantic_constants WHERE operator_name='__is_base_of'").fetchone()[0]
                    operands = db.execute('SELECT operand_id FROM source_semantic_constant_operands WHERE constant_id=? ORDER BY ordinal', (owner,)).fetchall()
                    check('reordered-types-originally-distinct', len(operands) == 2 and operands[0] != operands[1])
                    sql = f'UPDATE source_semantic_constant_operands SET operand_id=CASE ordinal WHEN 0 THEN {operands[1][0]} ELSE {operands[0][0]} END WHERE constant_id={owner}'
                check(name + '-changed', db.execute(sql).rowcount > 0)
                check(name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
