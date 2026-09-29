"""Validate the scoped upstream IPv6 hop-limit setter contract."""
import json,re
from pathlib import Path

def compact(text):
    return re.sub(r'\s+','',re.sub(r'/\*.*?\*/|//[^\n]*','',text,flags=re.S))

def check_contract(root,source):
    contract=json.loads((Path(root)/'config/ip6-hop-contract.json').read_text())
    header=(source/'src/include/lwip/prot/ip6.h').read_text()
    definitions=re.findall(r'^\s*#\s*define\s+IP6H_HOPLIM_SET\(([^)]*)\)\s*([^\n]*)',header,re.M)
    if len(definitions)!=1 or [x.strip() for x in definitions[0][0].split(',')]!=contract['parameters'] or compact(definitions[0][1])!=compact(contract['body']):
        raise RuntimeError('IPv6 hop-limit macro contract changed; review the override')
    unit=(source/contract['unit']).read_text()
    uncommented=re.sub(r'/\*.*?\*/|//[^\n]*',' ',unit,flags=re.S)
    tokens=compact(uncommented)
    if len(re.findall(r'\bIP6H_HOPLIM_SET\s*\(',uncommented))!=len(contract['calls']) or any(tokens.count(compact(call))!=1 for call in contract['calls']):
        raise RuntimeError('IPv6 setter call roles changed; review unit scoping')
    return contract


def managed_flags(root,source):
    check_contract(root,source)
    return ('-DDOTCC_LWIP_IP6_HOP_LIMIT=1', '--override-macro', 'IP6H_HOPLIM_SET=((hdr)->_hoplim = lwip_port_hop_limit((hl)))')


def native_flags(root,source):
    check_contract(root,source)
    return ['-include', str(Path(root)/'src/port/ip6-hop-limit-native.h')]
