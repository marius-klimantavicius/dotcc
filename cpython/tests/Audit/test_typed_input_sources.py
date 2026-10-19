"""Portable-PDB/Compile/PathMap rejection controls; no native/build subprocesses."""
import hashlib
import importlib.util
from pathlib import Path
import struct
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch
import uuid


spec = importlib.util.spec_from_file_location('typed_pdb_controls', Path(__file__).with_name('pdb_sources.py'))
mapper = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mapper)


class PdbSourceControls(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix='cpython-pdb-rejection-')
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.source = self.root/'Actual.cs'
        self.source.write_bytes(b'// actual control source bytes\n')
        self.pe = self.root/'synthetic.dll'
        self.pe.write_bytes(b'not-a-PE; the CodeView reader is explicitly mocked')
        self.pdb = self.root/'synthetic.pdb'
        self.guid = uuid.UUID('d7256611-a39e-408f-ae57-315b503f1150').bytes_le
        self.stamp = 42
        raw = b'RSDS'+self.guid+struct.pack('<I',1)+b'synthetic.pdb\0'
        self.pe_reader = SimpleNamespace(codeviews=lambda:[(self.stamp,raw)])
        self.write_pdb(str(self.source))

    def write_pdb(self, document, guid=None):
        blobs = bytearray(b'\0')
        def compressed(value):
            return bytes([value]) if value < 128 else bytes([0x80|(value>>8), value&255])
        def blob(data):
            index = len(blobs)
            blobs.extend(compressed(len(data))+data)
            return index
        parts = [blob(p.encode()) for p in document.split('/')]
        name = blob(b'/'+b''.join(compressed(p) for p in parts))
        checksum = blob(hashlib.sha256(self.source.read_bytes()).digest())
        heaps = {'#Pdb':(guid or self.guid)+struct.pack('<I',self.stamp),
            '#~':struct.pack('<IBBBBQQIHHHH',0,2,0,0,1,1<<48,0,1,name,1,checksum,2),
            '#Blob':bytes(blobs), '#GUID':uuid.UUID('8829d00f-11b8-4213-878b-770e8597ac16').bytes_le
                +uuid.UUID('3f5162f8-07c6-11d3-9053-00c04fa302a1').bytes_le}
        prefix = b'BSJB'+struct.pack('<HHII',1,1,0,4)+b'v1\0\0'+struct.pack('<HH',0,len(heaps))
        header_size = sum(8+((len(n)+1+3)&~3) for n in heaps)
        at = len(prefix)+header_size
        headers, content = bytearray(), bytearray()
        for name,data in heaps.items():
            encoded=name.encode()+b'\0';encoded+=b'\0'*((-len(encoded))%4)
            headers.extend(struct.pack('<II',at,len(data))+encoded)
            content.extend(data);at+=len(data)
        self.pdb.write_bytes(prefix+headers+content)

    def bind(self, maps=(), sources=None):
        with patch.object(mapper,'ManagedPE',return_value=self.pe_reader):
            return mapper.bind(self.pe,self.pdb,maps,sources or {self.source.resolve()})

    def test_synthetic_pdb_reads_exact_source_hash(self):
        report=self.bind()
        self.assertEqual(report['documents'][0]['checksum'],hashlib.sha256(self.source.read_bytes()).hexdigest())

    def test_wrong_pe_pdb_id_rejected(self):
        self.write_pdb(str(self.source),guid=b'\xff'*16)
        with self.assertRaises(AssertionError):self.bind()

    def test_source_changed_after_compilation_rejected(self):
        self.source.write_bytes(b'changed source')
        with self.assertRaises(AssertionError):self.bind()

    def test_explicit_actual_pathmap_resolves_only_compile_inputs(self):
        self.write_pdb('/logical/Actual.cs')
        result=self.bind([(str(self.root)+'/', '/logical/')])
        self.assertEqual(result['documents'][0]['path'],str(self.source))
        self.assertEqual(result['documents'][0]['pdbDocumentPath'],'/logical/Actual.cs')

    def test_invented_pathmap_without_actual_compile_input_rejected(self):
        self.write_pdb('/logical/Actual.cs')
        with self.assertRaisesRegex(AssertionError,'absent actual project Compile'):
            self.bind([(str(self.root)+'/', '/logical/')],{self.root/'unrelated.cs'})

    def test_ambiguous_pathmap_rejected(self):
        self.write_pdb('/logical/Actual.cs')
        maps=[(str(self.root)+'/', '/logical/')]*2
        with self.assertRaisesRegex(AssertionError,'ambiguous'):
            self.bind(maps)


if __name__ == '__main__':unittest.main()
