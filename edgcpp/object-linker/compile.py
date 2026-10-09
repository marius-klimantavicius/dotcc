#!/usr/bin/env python3
"""Compile one original source into a fresh owned SQLite semantic .dotobj."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
SCHEMA = Path(__file__).with_name('schema.sql')
ARRAYS = {
    'sourceFiles': ('source_files', ['id', 'path', 'parentId']),
    'types': ('types', ['id', 'kind', 'pointeeTypeId', 'elementTypeId', 'elementCount']),
    'storages': ('storages', ['id', 'sourceName', 'linkage', 'externalKey', 'definition', 'typeId', 'sourceTypeId', 'isThreadLocal', 'isConst', 'alignment', 'sourceFileId', 'line', 'column']),
    'storageData': ('storage_data', ['storageId', 'byteOffset', 'dataHex']),
    'relocations': ('relocations', ['storageId', 'byteOffset', 'targetStorageId', 'targetEntityId', 'addend']),
    'routineTypes': ('routine_types', ['typeId', 'returnTypeId', 'variadic', 'exceptionPolicy']),
    'routineParameters': ('routine_parameters', ['typeId', 'ordinal', 'parameterTypeId']),
    'sourceRoutineTypes': ('source_routine_types', ['typeId', 'returnTypeId', 'variadic', 'exceptionPolicy', 'parameterMode', 'objectTypeId', 'objectIsConst', 'objectIsVolatile', 'objectRefQualifier']),
    'sourceRoutineParameters': ('source_routine_parameters', ['typeId', 'ordinal', 'parameterTypeId']),
    'sourceRoutineAbis': ('source_routine_abis', ['sourceRoutineTypeId', 'returnPassing', 'captureProof']),
    'sourceRoutineAbiParameters': ('source_routine_abi_parameters', ['sourceRoutineTypeId', 'executionOrdinal', 'parameterRole', 'sourceOrdinal']),
    'entities': ('entities', ['id', 'sourceName', 'linkage', 'externalKey', 'definition',
                              'returnTypeId', 'sourceFileId', 'line', 'column', 'routineTypeId']),
    'parameters': ('parameters', ['entityId', 'ordinal', 'typeId', 'parameterRole', 'sourceOrdinal']),
    'functions': ('functions', ['entityId', 'bodyNodeId']),
    'abiRoutineOrigins': ('abi_routine_origins', ['entityId', 'primaryEntityId', 'entryKind', 'isAliasEntry', 'captureProof', 'targetEntityId']),
    'initializationActions': ('initialization_actions', ['id', 'entityId', 'phase', 'priority', 'ordinal', 'sourceFileId', 'line', 'column']),
    'sourceLifetimes': ('source_lifetimes', ['id', 'functionEntityId', 'parentLifetimeId', 'siblingOrdinal', 'parentObligationId', 'kind', 'sourceFileId', 'line', 'column', 'captureProof']),
    'sourceDestructionObligations': ('source_destruction_obligations', ['id', 'functionEntityId', 'lifetimeId', 'ordinal', 'destructorEntityId', 'sourceTypeId', 'sourceFileId', 'line', 'column', 'captureProof']),
    'expressionLifetimes': ('expression_lifetimes', ['nodeId', 'sourceLifetimeId', 'captureProof']),
    'baseSubobjectExpressions': ('base_subobject_expressions', ['nodeId', 'sourceBaseId']),
    'normalDestructions': ('normal_destructions', ['nodeId', 'sourceObligationId', 'captureProof']),
    'cleanupActions': ('cleanup_actions', ['id', 'functionEntityId', 'sourceObligationId', 'callbackEntityId', 'targetNodeId', 'captureProof']),
    'cleanupConditions': ('cleanup_conditions', ['actionId', 'flagBindingId', 'captureProof']),
    'cleanupStates': ('cleanup_states', ['id', 'functionEntityId', 'actionId', 'nextStateId']),
    'cleanupTransitions': ('cleanup_transitions', ['nodeId', 'stateId', 'captureProof', 'isUnreachable']),
    'cleanupFrameMarkers': ('cleanup_frame_markers', ['nodeId', 'kind', 'captureProof']),
    'generatedRoutineOrigins': ('generated_routine_origins', ['entityId', 'kind', 'captureProof', 'failurePolicy']),
    'dynamicInitializers': ('dynamic_initializers', ['id', 'actionId', 'storageId', 'sourceOrdinal', 'ordinal', 'captureProof', 'sourceFileId', 'line', 'column']),
    'dynamicInitializerStatements': ('dynamic_initializer_statements', ['initializerId', 'ordinal', 'nodeId']),
    'generatedRoutineScaffolds': ('generated_routine_scaffolds', ['entityId', 'ordinal', 'nodeId', 'bindingId', 'kind', 'captureProof']),
    'bindings': ('bindings', ['id', 'functionId', 'kind', 'ordinal', 'typeId', 'sourceName',
                              'scopeNodeId', 'sourceFileId', 'line', 'column', 'isSynthetic']),
    'nodes': ('nodes', ['id', 'functionId', 'kind', 'typeId', 'entityId', 'bindingId',
                        'operator', 'constantText', 'valueCategory', 'evaluationOrder',
                        'sourceFileId', 'line', 'column', 'fieldId', 'storageId']),
    'recordTypes': ('record_types', ['typeId', 'identityKey', 'sourceName', 'size', 'alignment', 'sourceFileId', 'line', 'column', 'layoutKind', 'sourceRecordId', 'representationKind', 'valueCopyKind', 'captureProof', 'completeTypeId']),
    'recordFields': ('record_fields', ['id', 'recordTypeId', 'ordinal', 'typeId', 'sourceName', 'byteOffset', 'sourceTypeId', 'sourceFileId', 'line', 'column', 'bitOffset', 'bitWidth', 'bitSigned', 'sourceFieldId', 'fieldRole', 'sourceBaseId', 'observedAlignment']),
    'sourceRecords': ('source_record_types', ['id', 'sourceTypeId', 'identityKey', 'sourceName', 'recordKind', 'isComplete', 'nativeSize', 'nativeAlignment', 'isTriviallyCopyable', 'capturePhase', 'sourceFileId', 'line', 'column', 'isPacked', 'maxMemberAlignment', 'explicitAlignment']),
    'exceptionTypes': ('exception_types', ['id', 'sourceTypeId', 'leafSourceTypeId', 'payloadTypeId', 'payloadSize', 'payloadAlignment', 'sourceDestructorEntityId', 'captureProof']),
    'exceptionBaseConversions': ('exception_base_conversions', ['exceptionTypeId', 'ordinal', 'baseSourceTypeId', 'byteOffset', 'isPublic', 'isAmbiguous', 'captureProof']),
    'throwExpressions': ('throw_expressions', ['nodeId', 'exceptionTypeId', 'destructorEntityId', 'initializationKind', 'captureProof']),
    'throwInitializationBoundaries': ('throw_initialization_boundaries', ['throwNodeId', 'cleanupPolicy', 'captureProof']),
    'throwCleanupTransitions': ('throw_cleanup_transitions', ['nodeId', 'throwNodeId', 'captureProof']),
    'throwObjectAddresses': ('throw_object_addresses', ['nodeId', 'throwNodeId', 'captureProof']),
    'throwMarkers': ('throw_markers', ['nodeId', 'throwNodeId', 'kind', 'captureProof']),
    'rethrowExpressions': ('rethrow_expressions', ['nodeId', 'captureProof']),
    'sourceRecordOperations': ('source_record_operations', ['sourceRecordId', 'bitwiseConstructionAllowed', 'bitwiseAssignmentAllowed', 'requiresReturnStorage', 'captureProof']),
    'recordValueTransfers': ('record_value_transfers', ['nodeId', 'kind', 'captureProof']),
    'sourceFields': ('source_record_fields', ['id', 'sourceRecordId', 'ordinal', 'sourceTypeId', 'sourceName', 'isMutable', 'hasNoUniqueAddress', 'bitWidth', 'sourceFileId', 'line', 'column', 'hasInitializer', 'isPacked', 'explicitAlignment']),
    'sourceBases': ('source_record_bases', ['id', 'sourceRecordId', 'ordinal', 'baseSourceTypeId', 'accessKind', 'isVirtual', 'sourceFileId', 'line', 'column']),
    'recordBaseSubobjects': ('record_base_subobjects', ['recordTypeId', 'sourceBaseId', 'completeBaseTypeId', 'physicalViewTypeId', 'byteOffset', 'extent', 'fieldId']),
    'memoryBarriers': ('memory_barriers', ['nodeId', 'isVolatile', 'hasVolatileKeyword', 'hasInlineKeyword', 'clobbersMemory', 'captureProof']),
    'memoryBarrierOperands': ('memory_barrier_operands', ['nodeId', 'ordinal', 'direction', 'constraintText', 'sourceName', 'sourceElementTypeId', 'elementSize', 'sourceShape', 'extentKind', 'fixedElementCount', 'extentBindingId', 'captureProof', 'sourceFileId', 'line', 'column']),
    'memoryBarrierDimensions': ('memory_barrier_dimensions', ['nodeId', 'operandOrdinal', 'ordinal', 'kind', 'fixedCount', 'extentBindingId', 'sourceFileId', 'line', 'column']),
    'edges': ('edges', ['parentId', 'role', 'ordinal', 'childId']),
    'macroDefinitions': ('macro_definitions', ['id', 'name', 'form', 'variadic', 'dynamic',
                        'definitionText', 'origin', 'sourceFileId', 'line', 'column']),
    'macroParameters': ('macro_parameters', ['definitionId', 'ordinal', 'name']),
    'macroEvents': ('macro_events', ['id', 'kind', 'name', 'definitionId', 'origin', 'sourceFileId', 'line', 'column']),
    'sourceTypes': ('source_types', ['id', 'kind', 'sourceName', 'underlyingTypeId',
                                   'executionTypeId', 'isConst', 'isVolatile', 'sourceFileId', 'line', 'column', 'elementCount']),
    'sourceFunctions': ('source_functions', ['entityId', 'returnSourceTypeId', 'capturePhase', 'routineSourceTypeId', 'functionKind']),
    'sourceParameters': ('source_parameters', ['entityId', 'ordinal', 'sourceTypeId', 'hasDefaultArgument']),
    'sourceDeclarations': ('source_declarations', ['id', 'kind', 'sourceName', 'sourceTypeId',
                          'entityId', 'bindingId', 'functionEntityId', 'sourceFileId', 'line', 'column']),
}


class FrontendOption(argparse.Action):
    """Retain preprocessing option order, including interleaved -D and -U."""
    def __call__(self, parser, namespace, values, option_string=None):
        options = list(getattr(namespace, self.dest, None) or [])
        options.append((option_string, values))
        setattr(namespace, self.dest, options)


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError('Duplicate wire key: ' + key)
        result[key] = value
    return result


def quote(value):
    return '"' + value.replace('"', '""') + '"'


def logical_hash(db):
    digest = hashlib.sha256(b'dotobj-semantic/1\n')

    def frame(value):
        digest.update(str(len(value)).encode('ascii') + b':' + value)

    for name, sql in db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' AND name<>'content_digest' ORDER BY name"):
        digest.update(b'T'); frame(name.encode('utf-8'))
        digest.update(b'S'); frame(sql.encode('utf-8'))
        keys = sorted((row[5], row[1]) for row in db.execute('PRAGMA table_info(' + quote(name) + ')') if row[5])
        if not keys:
            raise ValueError('No primary key: ' + name)
        for row in db.execute('SELECT * FROM ' + quote(name) + ' ORDER BY ' + ','.join(quote(k) for _, k in keys)):
            digest.update(b'R')
            for value in row:
                if value is None:
                    kind, data = b'5', b''
                elif isinstance(value, int):
                    kind, data = b'1', str(value).encode('ascii')
                elif isinstance(value, str):
                    kind, data = b'3', value.encode('utf-8')
                elif isinstance(value, bytes):
                    kind, data = b'4', value
                else:
                    raise ValueError('Unsupported SQLite cell')
                digest.update(kind); frame(data)
            digest.update(b'\n')
        digest.update(b'\n')
    return digest.hexdigest()


def package(wire, path, source, unit_id, provenance, started_ns):
    if set(wire) != {'formatVersion', 'profile', 'sourceLanguage', *ARRAYS}:
        raise ValueError('Unexpected semantic wire fields')
    if wire['formatVersion'] != 1 or wire['profile'] != 'scalar-control-v31':
        raise ValueError('Unsupported semantic wire version/profile')
    if wire['sourceLanguage'] not in ('c', 'c++'):
        raise ValueError('Unsupported source language')
    for kind, value in provenance:
        if kind == 'environment':
            captured_unit = json.loads(value).get('EDG_DOT_OBJECT_UNIT_ID')
            if captured_unit is not None and captured_unit != unit_id:
                raise ValueError('Owned translation unit identity differs from producer environment')
    source_hashes = {}
    with sqlite3.connect(path) as db:
        db.executescript(SCHEMA.read_text())
        # This is an unpublished staging database. Forward references and
        # cycles are expected; incremental deferred-FK bookkeeping scans child
        # tables whenever a missing parent arrives and becomes quadratic on
        # project-sized graphs. Validate all references once after the load.
        # CHECK/UNIQUE/NOT NULL constraints remain enforced on every insertion.
        db.execute('PRAGMA foreign_keys=OFF')
        db.execute('BEGIN IMMEDIATE')
        db.execute('INSERT INTO object_info VALUES(1,1,?,?,?,?,?,?,0,0,0,1,1,?)',
                   ('scalar-control-v31', 'portable-lp64-binary80-v2', 'dotobj-scalar-v1',
                    unit_id, str(source), sha(source), wire['sourceLanguage']))
        for index, (kind, value) in enumerate(provenance):
            db.execute('INSERT INTO provenance VALUES(?,?,?)', (index, kind, value))
        for name, (table, fields) in ARRAYS.items():
            if not isinstance(wire[name], list):
                raise ValueError('Expected array: ' + name)
            for record in wire[name]:
                if not isinstance(record, dict) or set(record) != set(fields):
                    raise ValueError('Invalid fields in ' + name + ': ' + str(record))
                values = [record[key] for key in fields]
                if name == 'storageData':
                    data = record['dataHex']
                    if (not isinstance(data, str) or not data or len(data) % 2
                            or any(c not in '0123456789abcdef' for c in data)):
                        raise ValueError('Noncanonical storage data hex')
                    values[-1] = bytes.fromhex(data)
                if name == 'sourceFiles':
                    file = Path(record['path'])
                    if file.is_file():
                        before = file.stat()
                        data = file.read_bytes()
                        after = file.stat()
                        if (before.st_mtime_ns, before.st_ctime_ns, before.st_size) != (after.st_mtime_ns, after.st_ctime_ns, after.st_size):
                            raise ValueError('Source changed during snapshot: ' + str(file))
                        if max(after.st_mtime_ns, after.st_ctime_ns) > started_ns:
                            raise ValueError('Source changed during compilation: ' + str(file))
                        digest = hashlib.sha256(data).hexdigest()
                        source_hashes[str(file)] = digest
                        values += [1, digest, data]
                    else:
                        values += [0, None, None]
                db.execute('INSERT INTO ' + quote(table) + ' VALUES(' + ','.join('?' for _ in values) + ')', values)
        errors = list(db.execute('PRAGMA foreign_key_check'))
        if errors:
            raise ValueError('Invalid semantic references: ' + str(errors[:10]))
        digest = logical_hash(db)
        db.execute('INSERT INTO content_digest VALUES(1,?,?)', ('dotobj-semantic-sha256-v1', digest))
        db.commit()
        db.execute('PRAGMA foreign_keys=ON')
        if db.execute('PRAGMA integrity_check').fetchone() != ('ok',):
            raise ValueError('SQLite integrity failure')
    for file, digest in source_hashes.items():
        if sha(file) != digest:
            raise ValueError('Source changed after snapshot: ' + file)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--work-dir', required=True, type=Path)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--std', choices=['c11', 'c++17', 'c++20'], default='c++17')
    parser.add_argument('--unit-id')
    parser.add_argument('-I', dest='frontend_options', action=FrontendOption)
    parser.add_argument('-D', dest='frontend_options', action=FrontendOption)
    parser.add_argument('-U', dest='frontend_options', action=FrontendOption)
    parser.add_argument('--preinclude', dest='frontend_options', action=FrontendOption)
    parser.add_argument('--exceptions', action=argparse.BooleanOptionalAction, default=False)
    parser.add_argument('--rtti', action=argparse.BooleanOptionalAction, default=False)
    args = parser.parse_args()
    source, output, work, compiler = (p.resolve() for p in (args.source, args.output, args.work_dir, args.compiler))
    if output.suffix != '.dotobj' or output.exists():
        parser.error('--output must be a new .dotobj file')
    if not source.is_file():
        parser.error('Source does not exist')
    work.mkdir(parents=True, exist_ok=False)
    output.parent.mkdir(parents=True, exist_ok=True)
    unit_id = args.unit_id or hashlib.sha256(str(source).encode('utf-8')).hexdigest()
    command = [str(compiler), '--' + args.std, '--gnu_version', '130300',
               '--clear_flag', 'use_predefined_macro_file']
    if args.std.startswith('c++'):
        command += ['--instantiate', 'used',
                    '--exceptions' if args.exceptions else '--no_exceptions',
                    '--rtti' if args.rtti else '--no_rtti']
    elif args.exceptions or args.rtti:
        parser.error('--exceptions and --rtti require C++')
    for option, value in args.frontend_options or []:
        if option in ('-I', '--preinclude'):
            command += [option, str(Path(value).resolve())]
        else:
            command += [option + value]
    command += ['--', str(source)]
    env = dict(os.environ, EDG_BASE=str(ROOT / 'upstream/bases/docker/dev-env/gcc'),
               EDG_DOT_OBJECT='1', EDG_DOT_OBJECT_UNIT_ID=unit_id, EDG_DOT_TARGET='portable-lp64-binary80-v2',
               EDG_DOT_NAMESPACE='@DotObject', EDG_DOT_CLASS='@Unit',
               EDG_DOT_EMIT='managedlib', EDG_DOT_INSTANCE='0', EDG_DOT_NEST='1',
               EDG_DOT_LIBC='none', EDG_DOT_RUNTIME='none', EDG_DOT_POSIX='0')
    before, compiler_hash = sha(source), sha(compiler)
    started_ns = time.time_ns()
    invocation = dict(compiler=str(compiler), compilerSha256=compiler_hash, arguments=command[1:],
                      source=str(source), sourceSha256=before, unitId=unit_id,
                      environment={key: value for key, value in env.items() if key.startswith('EDG_')})
    (work / 'invocation.json').write_text(json.dumps(invocation, indent=2) + '\n')
    with (work / 'wire.json').open('wb') as stdout, (work / 'stderr').open('wb') as stderr:
        result = subprocess.run(command, cwd=work, env=env, stdout=stdout, stderr=stderr)
    if result.returncode:
        raise RuntimeError('Frontend failed; preserved diagnostic: ' + str(work / 'stderr'))
    if sha(source) != before or sha(compiler) != compiler_hash:
        raise ValueError('Source/compiler changed during compilation')
    wire = json.loads((work / 'wire.json').read_text(), object_pairs_hook=unique_object)
    partial = work / 'object.partial.sqlite'
    provenance = [('producer', 'dot-gen-owned-semantic-v1'), ('compiler-sha256', compiler_hash),
                  ('arguments', json.dumps(command[1:])), ('packager-sha256', sha(__file__)),
                  ('schema-sha256', sha(SCHEMA)), ('environment', json.dumps(invocation['environment'], sort_keys=True))]
    package(wire, partial, source, unit_id, provenance, started_ns)
    # Stage on the destination filesystem, then publish atomically without
    # overwriting another build. Retain both staged copies as campaign evidence.
    pending = output.with_name(output.name + '.pending')
    with partial.open('rb') as incoming, pending.open('xb') as outgoing:
        import shutil
        shutil.copyfileobj(incoming, outgoing)
        outgoing.flush()
        os.fsync(outgoing.fileno())
    os.link(pending, output)
    report = dict(status='packaged-semantic-object', object=str(output), sha256=sha(output),
                  profile=wire['profile'], frontendInvocations=1, executionVerified=False,
                  capabilities=dict(prelowerSource=False, macros=False, lifetimes=False, lexicalCleanups=True, sourceDeclarations=True, macroDefinitions=True),
                  counts={name: len(wire[name]) for name in ARRAYS})
    (work / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps(report))


if __name__ == '__main__':
    main()
