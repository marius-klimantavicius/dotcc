"""Read portable PDB document hashes and bind its ID to the actual PE."""
from pathlib import Path
import hashlib
import struct
import uuid
import importlib.util

_spec = importlib.util.spec_from_file_location('pdb_pe_identity', Path(__file__).with_name('pe_identity.py'))
_pe_module = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_pe_module)
ManagedPE = _pe_module.ManagedPE

def streams(data):
    assert data[:4]==b'BSJB'
    cursor=16+struct.unpack_from('<I',data,12)[0]
    count=struct.unpack_from('<H',data,cursor+2)[0];cursor+=4
    result={}
    for _ in range(count):
        offset,size=struct.unpack_from('<II',data,cursor)
        end=data.index(b'\0',cursor+8)
        result[data[cursor+8:end].decode()]=data[offset:offset+size]
        cursor=(end+4)&~3
    return result

def compressed(data,at):
    first=data[at]
    if first<0x80:return first,at+1
    if first<0xc0:return ((first&0x3f)<<8)|data[at+1],at+2
    assert first<0xe0
    return ((first&0x1f)<<24)|(data[at+1]<<16)|(data[at+2]<<8)|data[at+3],at+4

def bind(pe_path,pdb_path,source_maps=(),compile_sources=None):
    pe_path,pdb_path=Path(pe_path),Path(pdb_path)
    data=pdb_path.read_bytes();heaps=streams(data);pdbid=heaps['#Pdb'][:20]
    pe=ManagedPE(pe_path);codeviews=[]
    for stamp,raw in pe.codeviews():
        assert raw[:4]==b'RSDS'
        codeviews.append({'guid':str(uuid.UUID(bytes_le=raw[4:20])),
            'stamp':stamp,'age':struct.unpack_from('<I',raw,20)[0],
            'path':raw[24:].rstrip(b'\0').decode()})
    assert len(codeviews)==1
    assert codeviews[0]['guid']==str(uuid.UUID(bytes_le=pdbid[:16]))
    assert codeviews[0]['stamp']==struct.unpack_from('<I',pdbid,16)[0]
    table=heaps['#~'];valid=struct.unpack_from('<Q',table,8)[0]
    counts={};at=24
    for i in range(64):
        if valid&(1<<i):counts[i]=struct.unpack_from('<I',table,at)[0];at+=4
    assert 48 in counts and not any(n for i,n in counts.items() if i<48)
    blobw=4 if table[6]&4 else 2;guidw=4 if table[6]&2 else 2
    def integer(width):
        nonlocal at
        value=int.from_bytes(table[at:at+width],'little');at+=width;return value
    def blob(index):
        size,start=compressed(heaps['#Blob'],index)
        return heaps['#Blob'][start:start+size]
    def guid(index):return str(uuid.UUID(bytes_le=heaps['#GUID'][(index-1)*16:index*16]))
    documents=[]
    for _ in range(counts[48]):
        name,algorithm,checksum,language=integer(blobw),integer(guidw),integer(blobw),integer(guidw)
        encoded=blob(name);sep=chr(encoded[0]);cursor=1;parts=[]
        while cursor<len(encoded):
            part,cursor=compressed(encoded,cursor);parts.append(blob(part).decode())
        path=Path(sep.join(parts))
        alg=guid(algorithm)
        assert alg in ('8829d00f-11b8-4213-878b-770e8597ac16','ff1816ec-aa5e-4d10-87f7-6f4963833460')
        kind='sha256' if alg.startswith('8829') else 'sha1'
        expected=blob(checksum).hex()
        document_path=str(path)
        # The normal semantic snapshot deliberately maps processed files back
        # to original display paths. Resolve only mappings explicitly supplied
        # by the actual compiled snapshot project's PathMap property.
        candidates=[];applies=False
        for physical,logical in source_maps:
            if document_path.startswith(logical):
                applies=True
                candidate=Path(physical+document_path[len(logical):])
                if compile_sources is None or candidate.resolve() in compile_sources:
                    candidates.append(candidate)
        if applies:
            assert compile_sources is not None,'PathMap proof needs actual project Compile inputs'
            assert len(candidates)==1,('ambiguous or absent actual project Compile input',document_path)
            path=candidates[0]
            assert path.is_file(),('absent actual physical compiler input',str(path))
        actual=hashlib.new(kind,path.read_bytes()).hexdigest()
        assert actual==expected,str(path)
        documents.append({'path':str(path),'pdbDocumentPath':document_path,'algorithm':kind,'checksum':expected,
            'sourceSha256':hashlib.sha256(path.read_bytes()).hexdigest(),'language':guid(language)})
    return {'actualPE':str(pe_path),'pdb':str(pdb_path),'pdbSha256':hashlib.sha256(data).hexdigest(),
        'portablePdbId':pdbid.hex(),'peCodeView':codeviews[0],'documents':documents,
        'allDocumentBytesMatchCompiledChecksums':True,'explicitSourceMaps':list(source_maps),'scope':'PE/PDB/actual compiled source byte identity; logical PathMap document preserved; no execution qualification'}
