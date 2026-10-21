#!/usr/bin/env python3
"""Pin manually selected source closures; reports only, no translated code.

Uses the approximate discovery index for references. The active dependency
lists below were selected by opening the original multipart routine bodies.
No compiled/managed equivalence or complete enabled-inventory claim is made.
"""
import hashlib
import json
from pathlib import Path

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]

# start line: active direct routine/macro dependencies and source obligations.
LEAVES = {
    33: ('signed_shift_right', 'Descending parts; mask then narrow then arithmetic sign-fill shift.'),
    50: ('', 'Reference overload; ascending part copies; same-storage alias permitted by source.'),
    62: ('', 'Descending parts; unsigned logical shift; signed work temporary preserves masked positive part.'),
    79: ('', 'Unsigned reference overload copies ascending parts just as the signed reference overload.'),
    91: ('sign_of', 'Sign/empty fill; high-part overflow scan then low-part accumulation; sign mismatch; value write precedes err write.'),
    202: ('conv_integer_value_to_host_large_integer', 'Clear overflow; extract; unsigned-to-signed negative-result or conversion error; return bits anyway.'),
    225: ('conv_integer_value_to_host_large_integer sign_of', 'Clear overflow; extract; conversion error or negative signed source; unsigned cast of returned bits.'),
    284: ('sign_of', 'Signedness short-circuits signs; unequal signs first; ascending lexicographic parts with ordered greater/less breaks.'),
    634: ('set_integer_value add_integer_values', 'Construct local one; unsigned add; deliberately ignore overflow out flag.'),
    711: ('sign_of INT_VALUE_PART_BASE', 'Descending carry loop; unsigned carry result or signed sign test AFTER destination writes; preserve alias-visible second sign.'),
    767: ('add_integer_values cmp_integer_values', 'Copy selected original signed operand; add with op_1 signedness; only mixed signedness overrides error using ordered compare.'),
    795: ('subtract_integer_values sign_of cmp_integer_values', 'Copy original op_1; subtract; mixed case reads op_2 sign after mutation; compare >0 for negative operand, <=0 otherwise.'),
    829: ('set_integer_value complement_integer_value shift_right_integer_value', 'Set zero; complement; unsigned right shift by total-minus-bits; bits>=1 source precondition.'),
    853: ('shift_left_integer_value shift_right_integer_value', 'Left shift first ignoring local error; then signed right shift with sign extension; size_t-to-int cast preserved.'),
    868: ('', 'Descending part loop; original integer promotions and narrowing; read-modify-write OR.'),
    882: ('', 'Descending part loop; original integer promotions and narrowing; read-modify-write AND.'),
    896: ('', 'Descending part loop; original integer promotions and narrowing; read-modify-write XOR.'),
    921: ('get_part', 'Quotient/remainder of count define offsets; ascending loop; lost-part and boundary-bit tests in source else-if order; mask then store; final error.'),
    980: ('sign_of get_part', 'Compute offsets; signed-and-sign_extend guard sets 16-bit fill; descending loop preserves in-place source reads; mask then store.'),
    1032: ('sign_of INT_VALUE_PART_BASE', 'Descending borrow loop; unsigned borrow or signed sign-difference test AFTER writes; preserve op_1/op_2 alias behavior.'),
    1094: ('set_integer_value subtract_integer_values', 'Set local zero; subtract source using signed operation; copy full result after helper error write.'),
    1109: ('', 'Descending loop; promoted complement then 0xffff mask then part narrowing.'),
    1178: ('clear_parts sign_of negate_integer_value copy_parts', 'Clear double-size work array; conditional local copies for negative operands and pointer retargeting; nested descending multiplication; copy low parts; high-part overflow scan; signed final negation/error handling.'),
    1333: ('', 'Descending raw-part multiplication with unsigned host-large carry and mask; no allocation; no overflow out flag.'),
    1358: ('INT_VALUE_PART_BASE', 'Ascending raw-part division; preserve old part before quotient write; carry uses modulus times base; divisor nonzero precondition.'),
    1383: ('INT_VALUE_PART_BASE', 'Descending loop includes index equal to normal part count; work buffers have extra part; signed borrow corrections.'),
    1415: ('clear_parts sign_of negate_integer_value set_integer_value cmp_integer_values copy_parts INT_VALUE_PART_BASE multiply_by_int_value_part special_subtract divide_by_int_value_part', 'Original work arrays, local divisor copy, optional dividend copy; divisor-zero/dividend-zero/smaller-dividend gotos; preserve output write order; normalize; trial quotient retry loop; subtract; denormalize; copy quotient then remainder; sign correction; common exit overwrites err.'),
    1634: ('divide_and_remainder_integer_values', 'Private helper receives op_1 as quotient output and a local remainder; selected multipart body only.'),
    1688: ('set_integer_value cmp_integer_values divide_and_remainder_integer_values', 'Construct -1; compare with actual op_2 signedness; zero result/clear error special case; otherwise helper with local quotient and op_1 remainder.'),
}

GROUPS = {
    'driver_configuration': 'cfe cfe_daemon cmd_line fe_init fe_wrapup target',
    'numeric': 'const_ints fixed_pt float_pt floating literals folding',
    'preprocessing_input': 'lexical macro preproc pch pragma sys_predef unicode_name_fsm',
    'declarations_lookup': 'attribute class_decl decl_inits decl_spec declarator decls def_arg disambig func_def lookup overload scope_stk symbol_ref symbol_tbl',
    'expressions_statements_constexpr': 'expr exprutil interpret statements extasm ms_attrib',
    'templates_modules': 'templates modules ifc_modules ifc_modules_read ifc_modules_write ifc_modules_templ ifc_map_functions ifc_map_functions_acc ifc_map_functions_dbg ifc_map_functions_mut ifc_map_functions_val',
    'identity_il_runtime': 'il il_alloc il_display il_read il_write il_walk il_to_str mem_manage host_envir error src_seq trans_unit trans_copy trans_corresp types',
    'compiled_lowering_requires_reachability_audit': 'inline layout lower_c99 lower_eh lower_il lower_init lower_name',
    'backend_output_outside_initial_deliverable_dependencies_unresolved': 'c_gen_be cp_gen_be',
    'preserved_local_backend_overlay': 'dot_gen_be dot_sys_predef',
    'msvc_configuration_backlog': 'ms_metadata',
}


def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def main():
    candidates = [json.loads(x) for x in (OUT/'routine-candidates.jsonl').open()]
    rows = []
    found = set()
    for r in candidates:
        if r['file']=='edgcpp/upstream/src/const_ints.c' and r['start'] in LEAVES:
            deps, note = LEAVES[r['start']]
            found.add(r['start'])
            rows.append(dict(r, active_direct_dependencies=deps.split(),native_inspection_note=note,
                active_profile='cpfe: INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0; LP64 host',
                exclusions='Scalar representation alternatives retained in source/branch inventory but inactive in this build',
                source_effect_contract='Only passed storage and local values; no global writes/allocation/diagnostics in selected bodies',
                branch_review_status='Native obligations recorded; full C#/source operation mapping and independent review pending',
                managed_review_status='No managed files opened by this research task; no parity qualification'))
    if found != set(LEAVES):
        raise RuntimeError('Missing selected source routines: '+str(set(LEAVES)-found))
    macros = [json.loads(x) for x in (OUT/'macro-definitions.jsonl').open()]
    macro_names={'signed_shift_right','sign_of','INT_VALUE_PART_BASE','get_part','clear_parts','copy_parts'}
    macro_rows=[m for m in macros if m['file'] in ('edgcpp/upstream/src/const_ints.c','edgcpp/upstream/src/const_ints.h') and m['name'] in macro_names]
    obj=dict(status='SOURCE-DEFINED INITIAL CLOSURE; native research only; managed qualification pending',
        file='edgcpp/upstream/src/const_ints.c',file_sha256=digest(ROOT/'edgcpp/upstream/src/const_ints.c'),
        required_representation='an_integer_value embeds eight unsigned short parts, highest first; host ptrdiff_t/size_t 64 bit',
        active_macro_selection='signed_shift_right and sign_of select non-host-integer branches; other listed macros are multipart definitions',
        state_dependencies='Passed storage, out flags, local copy/array storage and pinned widths; caller preconditions/alias contracts remain source-defined',
        complete_call_graph_claim=False, routines=rows,macro_definitions_with_all_alternatives=macro_rows,
        next_closure='IL-aware constants/types and runtime target/checking/minmax initialization (separate width research); then formatter/float dependencies')
    (OUT/'const_ints-initial-closure.json').write_text(json.dumps(obj,indent=2,sort_keys=True)+'\n')
    files=[json.loads(x) for x in (OUT/'files.jsonl').open()]
    groups={name:group for group,names in GROUPS.items() for name in names.split()}
    lines=['file\tsha256\tgroup\tcpfe_membership\tdisposition\n']
    for r in files:
        p=Path(r['file'])
        group=groups.get(p.stem,'header_macro_type_table_configuration_requires_review')
        membership='compiled-source' if 'cpfe' in r['compiled_targets'] else 'not-a-cpfe-translation-unit'
        disposition='present; unstarted; enabled-routine/type/macro reachability unresolved'
        lines.append('\t'.join([r['file'],r['sha256'],group,membership,disposition])+'\n')
    (OUT/'source-units.tsv').write_text(''.join(lines))
    print(json.dumps(dict(selected_leaf_routines=len(rows),source_files_accounted=len(files))))


if __name__=='__main__':
    main()
