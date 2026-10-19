"""Small read-only PE/CLI identity reader; Python standard library only.

Extracted from the A15 identity calculation, with bounded PE section access in
place of a third-party parser. It never loads or executes the managed assembly.
"""
from pathlib import Path
import hashlib
import struct
import uuid


class ManagedPE:
    def __init__(self, path):
        self.data = Path(path).read_bytes()
        assert self.data[:2] == b'MZ', 'not a PE image'
        pe = struct.unpack_from('<I', self.data, 0x3c)[0]
        assert self.data[pe:pe+4] == b'PE\0\0', 'invalid PE signature'
        sections = struct.unpack_from('<H', self.data, pe+6)[0]
        optional_size = struct.unpack_from('<H', self.data, pe+20)[0]
        optional = pe+24
        magic = struct.unpack_from('<H', self.data, optional)[0]
        assert magic in (0x10b, 0x20b), 'unsupported PE optional header'
        directory = optional + (112 if magic == 0x20b else 96)
        count = struct.unpack_from('<I', self.data, directory-4)[0]
        assert count >= 15 and directory+count*8 <= optional+optional_size, 'missing PE data directories'
        self.directories = [struct.unpack_from('<II',self.data,directory+i*8) for i in range(count)]
        self.header_size = struct.unpack_from('<I',self.data,optional+60)[0]
        section_table = optional+optional_size
        self.sections = [struct.unpack_from('<IIII',self.data,section_table+i*40+8) for i in range(sections)]

    def get_data(self, rva, size):
        assert rva >= 0 and size >= 0
        if rva < self.header_size:
            assert rva+size <= self.header_size and rva+size <= len(self.data)
            return self.data[rva:rva+size]
        for virtual_size, address, raw_size, raw_offset in self.sections:
            if address <= rva < address+max(virtual_size,raw_size):
                offset = rva-address
                assert offset+size <= raw_size and raw_offset+offset+size <= len(self.data), 'PE RVA outside file data'
                return self.data[raw_offset+offset:raw_offset+offset+size]
        raise AssertionError('unmapped PE RVA')

    def codeviews(self):
        rva, size = self.directories[6]
        assert size and size%28 == 0, 'missing/invalid PE debug directory'
        debug = self.get_data(rva,size)
        result = []
        for at in range(0,size,28):
            _, stamp, _, _, kind, length, address, _ = struct.unpack_from('<IIHHIIII',debug,at)
            if kind == 2:
                result.append((stamp,self.get_data(address,length)))
        return result


def metadata_streams(data):
    assert data[:4] == b'BSJB', 'invalid CLI metadata signature'
    cursor = 16+struct.unpack_from('<I',data,12)[0]
    count = struct.unpack_from('<H',data,cursor+2)[0]
    cursor += 4
    streams = {}
    for _ in range(count):
        offset, length = struct.unpack_from('<II',data,cursor)
        end = data.index(b'\0',cursor+8)
        name = data[cursor+8:end].decode()
        assert name not in streams and offset+length <= len(data), 'invalid metadata stream'
        streams[name] = data[offset:offset+length]
        cursor = (end+4)&~3
    return streams


def pe_identity(path):
    path = Path(path).resolve()
    pe = ManagedPE(path)
    cor, _ = pe.directories[14]
    metadata_rva, metadata_size = struct.unpack('<II',pe.get_data(cor+8,8))
    streams = metadata_streams(pe.get_data(metadata_rva,metadata_size))
    tables = streams.get('#~',streams.get('#-'))
    heaps = tables[6]
    valid = struct.unpack_from('<Q',tables,8)[0]
    assert valid&1 and struct.unpack_from('<I',tables,24)[0] == 1, 'invalid CLI Module table'
    first = 24+4*valid.bit_count()
    string_width, guid_width = (4 if heaps&1 else 2), (4 if heaps&2 else 2)
    guid_index = int.from_bytes(tables[first+2+string_width:first+2+string_width+guid_width],'little')
    assert guid_index > 0
    mvid = uuid.UUID(bytes_le=streams['#GUID'][16*(guid_index-1):16*guid_index])
    return {'path':str(path),'sha256':hashlib.sha256(pe.data).hexdigest(),'mvid':str(mvid),
            'entryPointToken':hex(struct.unpack('<I',pe.get_data(cor+20,4))[0])}
