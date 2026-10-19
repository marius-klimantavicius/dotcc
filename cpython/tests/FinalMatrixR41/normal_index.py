"""Bind finished ordinary Upstream executions for the normal campaign hook.

This only reads completed artifacts. The normal hook independently repeats the
strict completed-input review and released acceptance gate; an index is no pass.
"""
import argparse
from pathlib import Path
from run import ART,A32,read,ref,verify,write


def prepare(paths, output):
    cells = {}
    receipt = None
    for path in paths:
        path = Path(path).resolve()
        cell = read(path)
        assert cell['profile'] == 'upstream-tests-static' and cell['phase'] == 'upstream'
        directory = path.parent
        proof_path = directory / 'Upstream-actual-inputs.json'
        proof = read(proof_path)
        publication_ref = proof['publication']; verify(publication_ref)
        publication = read(publication_ref['path'])
        verify(publication['receipt'])
        if receipt is None: receipt = publication['receipt']
        assert receipt == publication['receipt'], 'Mixed translation receipts'
        library = proof['library']['identity']
        tag = proof['profile'] + '-' + proof['form'] + '-' + library['sha256'][:16]
        evidence_path = A32 / 'd10' / tag / 'evidence.json'
        evidence = read(evidence_path)
        for name in ('compiled_proof','actual_metadata','typed_data','address_review'):
            verify(evidence[name])
        compiled = read(evidence['compiled_proof']['path'])
        assert compiled['assembly_sha256'] == library['sha256'] and compiled['mvid'] == library['mvid']
        assert compiled['identity']['receipt_sha256'] == receipt['sha256']
        artifacts = dict(rawReport=ref(directory / 'full-required.json'),
            rawJournal=ref(directory / 'full-required.journal.jsonl'),actualInputProof=ref(proof_path),
            executionProof=ref(directory / 'primary-execution-input.json'),commands=ref(directory / 'commands.json'),
            configurationCoverage=ref(directory / 'two-mode-coverage.json'))
        key = proof['form'] + '-' + proof['mode']
        assert key not in cells, 'Duplicate actual upstream cell'
        cells[key] = dict(artifacts=artifacts,a32Evidence=ref(evidence_path),matrixCell=ref(path))
    assert cells and receipt is not None
    return write(output,dict(profile='upstream-tests-static',receipt=receipt,cells=cells,
        scope='Completed actual ordinary Upstream inputs only; normal campaign strict proof and acceptance reviews remain mandatory'))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--cell',action='append',required=True,type=Path)
    parser.add_argument('--output',required=True,type=Path)
    args = parser.parse_args()
    print(prepare(args.cell,args.output))
