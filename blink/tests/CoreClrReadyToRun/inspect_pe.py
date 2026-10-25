#!/usr/bin/env python3
"""Read real CLI/R2R PE files without loading or executing their contents.

Format references (runtime v10.0.12, MIT):
  docs/design/coreclr/botr/readytorun-format.md
  src/coreclr/inc/readytorun.h
  src/coreclr/tools/aot/ILCompiler.Reflection.ReadyToRun/{NativeArray,NativeReader,ReadyToRunReader}.cs
The NativeArray traversal and unsigned decoding below follow those definitions.
This inspector proves payload presence, not runtime selection of every method.
"""
import argparse
import hashlib
import json
from pathlib import Path
import struct

SECTIONS = {100: 'CompilerIdentifier', 101: 'ImportSections', 102: 'RuntimeFunctions',
    103: 'MethodDefEntryPoints', 104: 'ExceptionInfo', 105: 'DebugInfo',
    106: 'DelayLoadMethodCallThunks', 108: 'AvailableTypes', 109: 'InstanceMethodEntryPoints',
    110: 'InliningInfo', 111: 'ProfileDataInfo', 112: 'ManifestMetadata',
    113: 'AttributePresence', 114: 'InliningInfo2', 115: 'ComponentAssemblies',
    116: 'OwnerCompositeExecutable', 117: 'PgoInstrumentationData',
    118: 'ManifestAssemblyMvids', 119: 'CrossModuleInlineInfo', 120: 'HotColdMap',
    121: 'MethodIsGenericMap', 122: 'EnclosingTypeMap', 123: 'TypeGenericInfoMap'}


def digest(data):
    return hashlib.sha256(data).hexdigest()


class PE:
    def __init__(self, path):
        self.path = Path(path)
        self.data = self.path.read_bytes()
        if self.take(0, 2) != b'MZ':
            raise ValueError('Expected a PE image: ' + str(path))
        pe = self.unpack('<I', 60)[0]
        if self.take(pe, 4) != b'PE\0\0':
            raise ValueError('PE signature differs')
        self.machine, count = self.unpack('<HH', pe + 4)
        optional = pe + 24
        size = self.unpack('<H', pe + 20)[0]
        magic = self.unpack('<H', optional)[0]
        if magic not in (0x10b, 0x20b):
            raise ValueError('Unsupported PE optional header')
        directories = optional + (112 if magic == 0x20b else 96)
        self.sections = []
        for index in range(count):
            start = optional + size + index * 40
            name = self.take(start, 8).rstrip(b'\0').decode('ascii')
            virtual_size, rva, raw_size, raw = self.unpack('<IIII', start + 8)
            characteristics = self.unpack('<I', start + 36)[0]
            self.take(raw, raw_size)
            self.sections.append(dict(name=name, rva=rva, virtual_size=virtual_size,
                raw_offset=raw, raw_size=raw_size, characteristics=characteristics,
                executable=bool(characteristics & 0x20000000)))
        self.cli_rva, self.cli_size = self.unpack('<II', directories + 14 * 8)

    def take(self, offset, size):
        if offset < 0 or size < 0 or offset + size > len(self.data):
            raise ValueError('Read outside PE bytes')
        return self.data[offset:offset + size]

    def unpack(self, fmt, offset):
        return struct.unpack(fmt, self.take(offset, struct.calcsize(fmt)))

    def locate(self, rva, size):
        for section in self.sections:
            delta = rva - section['rva']
            if 0 <= delta and delta + size <= section['raw_size']:
                return section['raw_offset'] + delta, section
        raise ValueError('RVA range has no file-backed section: ' + hex(rva))

    def rva_bytes(self, rva, size):
        offset, _ = self.locate(rva, size)
        return self.take(offset, size)

    def methoddef_metadata(self, rva, size):
        data = self.rva_bytes(rva, size)
        if data[:4] != b'BSJB':
            raise ValueError('CLI metadata signature differs')
        version_size = struct.unpack_from('<I', data, 12)[0]
        cursor = (16 + version_size + 3) & ~3
        streams = struct.unpack_from('<H', data, cursor + 2)[0]
        cursor += 4
        for _ in range(streams):
            offset, length = struct.unpack_from('<II', data, cursor)
            end = data.index(0, cursor + 8)
            name = data[cursor + 8:end]
            cursor = (end + 4) & ~3
            if name in (b'#~', b'#-'):
                if offset + length > len(data):
                    raise ValueError('CLI table stream outside metadata')
                heaps = data[offset + 6]
                valid = struct.unpack_from('<Q', data, offset + 8)[0]
                rows = offset + 24
                counts = {}
                for table in range(64):
                    if valid & (1 << table):
                        counts[table] = struct.unpack_from('<I', data, rows)[0]
                        rows += 4
                simple = lambda table: 4 if counts.get(table, 0) >= 65536 else 2
                def coded(tables, bits):
                    return 4 if max(counts.get(t, 0) for t in tables) >= (1 << (16 - bits)) else 2
                string, guid, blob = [4 if heaps & bit else 2 for bit in (1, 2, 4)]
                widths = [2 + string + 3 * guid,
                    coded((0, 26, 35, 1), 2) + 2 * string,
                    4 + 2 * string + coded((2, 1, 27), 2) + simple(4) + simple(6),
                    simple(4), 2 + string + blob, simple(6)]
                rows += sum(counts.get(t, 0) * width for t, width in enumerate(widths))
                width = 8 + string + blob + simple(8)
                count = counts.get(6, 0)
                body_count = code_bytes = 0
                for method in range(count):
                    body, implementation, attributes = struct.unpack_from('<IHH', data, rows + method * width)
                    if not body or implementation & 3 or attributes & 0x400:
                        continue
                    first = self.rva_bytes(body, 1)[0]
                    if first & 3 == 2:
                        header_size, code_size = 1, first >> 2
                    elif first & 3 == 3:
                        flags, _, code_size = struct.unpack_from('<HHI', self.rva_bytes(body, 12))
                        header_size = (flags >> 12) * 4
                    else:
                        raise ValueError('Unrecognized IL method body header')
                    self.rva_bytes(body, header_size + code_size)
                    body_count += 1
                    code_bytes += code_size
                return dict(methoddef_count=count, il_method_body_count=body_count,
                    il_method_code_bytes=code_bytes)
        raise ValueError('No CLI table stream')


def unsigned(data, offset):
    if not 0 <= offset < len(data):
        raise ValueError('Native unsigned offset outside section')
    first = data[offset]
    for size in range(1, 5):
        if not first & (1 << (size - 1)):
            if offset + size > len(data):
                raise ValueError('Native unsigned extends outside section')
            value = first >> size
            for index in range(1, size):
                value |= data[offset + index] << (8 * index - size)
            return value, offset + size
    if not first & 16 and offset + 5 <= len(data):
        return struct.unpack_from('<I', data, offset + 1)[0], offset + 5
    raise ValueError('Unsupported native unsigned encoding')


def entrypoints(data):
    header, base = unsigned(data, 0)
    count, encoding = header >> 2, header & 3
    if count > 0x1000000:
        raise ValueError('NativeArray count outside CLI token space')
    width = (1, 2, 4, 4)[encoding]
    entries = []
    for index in range(count):
        start = base + width * (index // 16)
        if start + width > len(data):
            raise ValueError('NativeArray block index outside section')
        offset = base + int.from_bytes(data[start:start + width], 'little')
        found = True
        for bit in (8, 4, 2, 1):
            node, after = unsigned(data, offset)
            if index & bit:
                if node & 2:
                    offset += node >> 2
                    continue
            elif node & 1:
                offset = after
                continue
            if node & 3 == 0 and node >> 2 == index & 15:
                offset = after
                break
            found = False
            break
        if found:
            value, _ = unsigned(data, offset)
            function = value >> (2 if value & 1 else 1)
            entries.append(dict(methoddef_token=f'0x{0x06000001 + index:08x}',
                runtime_function_index=function, has_fixups=bool(value & 1)))
    return count, entries


def inspect(path, detail=False):
    pe = PE(path)
    result = dict(path=str(pe.path.resolve()), sha256=digest(pe.data), size=len(pe.data),
        machine=f'0x{pe.machine:04x}', pe_sections=pe.sections, managed=bool(pe.cli_rva),
        ready_to_run=False, native_payload_proven=False)
    if not pe.cli_rva:
        return result
    cli = pe.rva_bytes(pe.cli_rva, pe.cli_size)
    metadata_rva, metadata_size, flags, token = struct.unpack_from('<IIII', cli, 8)
    native_rva, native_size = struct.unpack_from('<II', cli, 64)
    metadata = pe.methoddef_metadata(metadata_rva, metadata_size)
    method_count = metadata['methoddef_count']
    result['cli'] = dict(flags=flags, entrypoint_token=f'0x{token:08x}',
        **metadata, metadata_rva=metadata_rva, metadata_size=metadata_size,
        managed_native_header_rva=native_rva, managed_native_header_size=native_size)
    if not native_rva:
        return result
    header = pe.rva_bytes(native_rva, native_size)
    signature, major, minor, r2r_flags, count = struct.unpack_from('<IHHII', header)
    if signature != 0x00525452:
        raise ValueError('ManagedNativeHeader is not RTR: ' + str(path))
    if 16 + count * 12 > len(header):
        raise ValueError('R2R directory outside ManagedNativeHeader')
    sections, payloads = [], {}
    for index in range(count):
        kind, rva, size = struct.unpack_from('<III', header, 16 + 12 * index)
        data = pe.rva_bytes(rva, size)
        if kind in payloads:
            raise ValueError('Duplicate R2R section')
        payloads[kind] = data
        sections.append(dict(type=kind, name=SECTIONS.get(kind, 'Unknown'), rva=rva,
            size=size, sha256=digest(data)))
    r2r = dict(signature='RTR\u0000', version=f'{major}.{minor}', flags=r2r_flags,
        sections=sections, compiler_identifier=payloads.get(100, b'').rstrip(b'\0').decode('ascii'))
    result.update(ready_to_run=True, r2r=r2r)
    functions = []
    native_hash = hashlib.sha256()
    code_bytes = 0
    if 102 in payloads:
        # Linux x64 uses IMAGE_FILE_MACHINE_AMD64 XOR native OS override.
        if pe.machine not in (0x8664, 0x8664 ^ 0x7b79):
            raise ValueError('RuntimeFunctions decoder supports x64 only')
        data = payloads[102]
        if len(data) % 12:
            raise ValueError('x64 RuntimeFunctions size is not a multiple of 12')
        for offset in range(0, len(data), 12):
            begin, end, unwind = struct.unpack_from('<III', data, offset)
            if end <= begin:
                raise ValueError('RuntimeFunction has no code range')
            _, section = pe.locate(begin, end - begin)
            if not section['executable']:
                raise ValueError('RuntimeFunction points outside executable section')
            code = pe.rva_bytes(begin, end - begin)
            pe.locate(unwind, 1)
            native_hash.update(code)
            code_bytes += len(code)
            functions.append(dict(index=offset // 12, begin_rva=begin, end_rva=end,
                unwind_rva=unwind, pe_section=section['name'], sha256=digest(code)))
    entries = []
    if 103 in payloads:
        slots, entries = entrypoints(payloads[103])
        if slots > method_count:
            raise ValueError('R2R MethodDef slots exceed CLI MethodDef table')
        for entry in entries:
            if entry['runtime_function_index'] >= len(functions):
                raise ValueError('MethodDef entrypoint has no RuntimeFunction')
        r2r.update(methoddef_slots=slots, methoddef_entrypoint_count=len(entries),
            methoddef_entrypoints=entries if detail else entries[:8])
    r2r.update(runtime_function_count=len(functions), native_code_bytes=code_bytes,
        native_code_sha256=native_hash.hexdigest(),
        runtime_functions=functions if detail else functions[:8])
    result['native_payload_proven'] = bool(entries and functions and code_bytes)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('images', type=Path, nargs='+')
    parser.add_argument('--detail', action='store_true')
    parser.add_argument('--require-native-payload', action='store_true')
    args = parser.parse_args()
    results = [inspect(path, args.detail) for path in args.images]
    print(json.dumps(results, indent=2))
    if args.require_native_payload and not all(row['native_payload_proven'] for row in results):
        return 1
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
