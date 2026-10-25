#!/usr/bin/env python3
"""Positive regression using two real, independently published valid PE images."""
import argparse
from pathlib import Path
import sys
sys.dont_write_bytecode = True
from inspect_pe import inspect


def verify_pair(il, r2r):
    control, compiled = inspect(il, detail=True), inspect(r2r, detail=True)
    assert control['managed'] and compiled['managed']
    assert control['cli']['il_method_body_count'] > 0
    assert compiled['cli']['il_method_body_count'] == control['cli']['il_method_body_count']
    assert compiled['cli']['methoddef_count'] == control['cli']['methoddef_count']
    assert control['cli']['managed_native_header_rva'] == 0
    assert control['cli']['managed_native_header_size'] == 0
    assert not control['ready_to_run'] and not control['native_payload_proven']
    assert compiled['cli']['managed_native_header_rva'] > 0
    assert compiled['r2r']['signature'] == 'RTR\0'
    assert compiled['r2r']['version'] == '16.0'
    assert compiled['native_payload_proven']
    assert compiled['r2r']['methoddef_entrypoint_count'] > 0
    assert compiled['r2r']['runtime_function_count'] > 0
    assert compiled['r2r']['native_code_bytes'] > 0
    assert compiled['sha256'] != control['sha256']
    assert not {115, 116}.intersection(s['type'] for s in compiled['r2r']['sections'])
    by_index = {r['index']: r for r in compiled['r2r']['runtime_functions']}
    for entry in compiled['r2r']['methoddef_entrypoints']:
        assert by_index[entry['runtime_function_index']]['end_rva'] > by_index[entry['runtime_function_index']]['begin_rva']
    return dict(passed=True, valid_images_only=True, il_sha256=control['sha256'],
        r2r_sha256=compiled['sha256'], il_method_bodies=control['cli']['il_method_body_count'],
        compiled_methoddef_entrypoints=compiled['r2r']['methoddef_entrypoint_count'],
        runtime_functions=compiled['r2r']['runtime_function_count'])


def main():
    import json
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--il', type=Path, required=True)
    parser.add_argument('--r2r', type=Path, required=True)
    args = parser.parse_args()
    print(json.dumps(verify_pair(args.il, args.r2r), indent=2))


if __name__ == '__main__':
    main()
