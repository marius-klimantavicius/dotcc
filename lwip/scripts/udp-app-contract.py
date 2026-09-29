"""Guard local original TFTP contracts used by the owning include wrapper."""
import hashlib
import json
import re


def compact(text):
    return re.sub(r'\s+', '', re.sub(r'/\*.*?\*/|//[^\n]*', '', text, flags=re.S))


def check_contract(root, source):
    contract = json.loads((root / 'config/udp-app-contract.json').read_text())
    path = source / contract['unit']
    data = path.read_bytes()
    body = compact(data.decode())
    for name, expected in contract['contracts'].items():
        if body.count(compact(expected)) != 1:
            raise RuntimeError('TFTP local contract changed; review owning wrapper: ' + name)
    return {'unit': contract['unit'], 'sha256': hashlib.sha256(data).hexdigest(),
            'pinned_sha256': contract['pinned_sha256'],
            'contracts': list(contract['contracts']), 'scope': contract['scope'],
            'reason': contract['reason'], 'removal_condition': contract['removal_condition']}
