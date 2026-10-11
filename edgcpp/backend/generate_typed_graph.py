#!/usr/bin/env python3
"""Generate a read-only specialization of EDG's configured IL pointer walker.

The stock walker supplies the discriminated reference traversal. Scalar fields
come from the configured declarations, never raw struct bytes or pointer values.
Use a preprocessor-only configured-il input; this script does not build EDG.
"""
from pathlib import Path
import argparse
import hashlib
import json
import re


def statements(body):
    start = depth = 0
    for index, char in enumerate(body):
        depth += (char == '{') - (char == '}')
        if char == ';' and depth == 0:
            yield body[start:index].strip()
            start = index + 1


def block(text, start):
    depth, end = 1, start
    while depth:
        depth += (text[end] == '{') - (text[end] == '}')
        end += 1
    return text[start:end - 1], end


def inventory(text):
    records = {}
    for match in re.finditer(r'(?:typedef\s+)?(struct|union)\s+(\w+)\s*\{', text):
        body, _ = block(text, match.end())
        records[match[2]] = (match[1], body)
    aliases = {}
    for match in re.finditer(r'typedef\s+([^;{}]+?)\s+(\w+)\s*;', text):
        aliases[match[2]] = ' '.join(match[1].split())
    for match in re.finditer(r'typedef\s+((?:struct\s+)?\w+\s*\*+)\s*(\w+)\s*;', text):
        aliases[match[2]] = ' '.join(match[1].split())
    return records, aliases


def fields(body):
    for statement in statements(body):
        embedded = re.match(r'(struct|union)\s*\{', statement)
        if embedded:
            nested, end = block(statement, embedded.end())
            yield statement[end:].strip(), embedded[1], nested
            continue
        match = re.match(r'(.+?)\b(\w+)\s*((?:[:\[,].*)?)$', statement, re.S)
        if match:
            typ, suffix = ' '.join(match[1].split()), match[3].strip()
            if suffix.startswith(','):
                yield match[2], typ, ''
                for item in suffix[1:].split(','):
                    yield item.strip(), typ, ''
            else:
                yield match[2], typ, suffix
        else:
            yield '$unparsed', 'gap', statement


def generate(walker, configured):
    records, aliases = inventory(configured)
    gaps = set()
    retained = set()
    configured_disabled = set()
    pointer_unions = set()

    def resolve(typ):
        seen = set()
        while typ in aliases and typ not in seen:
            seen.add(typ)
            typ = aliases[typ]
        return typ

    def pointer_only_union(body):
        members = list(fields(body))
        return bool(members) and all('*' in resolve(typ) or typ.endswith('_ptr') for name, typ, suffix in members)

    def scalar_lines(body, prefix, record, active=()):
        result = []
        for name, typ, suffix in fields(body):
            path = prefix + name
            if typ == 'union':
                if pointer_only_union(suffix):
                    pointer_unions.add((record, path))
                else:
                    gaps.add((record, path, 'active-union-selection'))
                continue
            if typ == 'struct':
                result.extend(scalar_lines(suffix, path + '.', record, active))
                continue
            resolved = resolve(typ)
            if '*' in resolved or '*' in typ or '(' in typ or typ.endswith('_ptr'):
                continue  # Stock walk supplies pointer fields and string lengths.
            if typ in records:
                if typ in active:
                    gaps.add((record, path, 'recursive-embedded-record'))
                elif records[typ][0] == 'union':
                    if pointer_only_union(records[typ][1]):
                        pointer_unions.add((record, path))
                    else:
                        gaps.add((record, path, 'active-union-selection'))
                else:
                    result.extend(scalar_lines(records[typ][1], path + '.', record, active + (typ,)))
                continue
            if name == '$unparsed' or ',' in suffix or typ == 'gap':
                gaps.add((record, path, suffix))
                continue
            expr = 'eptr->' + path
            # Arrays retain ordered scalar elements, including integer limbs.
            if suffix.startswith('['):
                result.append('for (unsigned graph_index=0; graph_index<sizeof(' + expr + ')/sizeof((' + expr + ')[0]); ++graph_index) scalar("' + path + '", (' + expr + ')[graph_index], graph_index);')
            else:
                result.append('scalar("' + path + '", ' + expr + ');')
            retained.add((record, path))
        return result

    start = walker.index('/* The name is provided by a macro')
    end = walker.index('#ifdef WALK_ORPHANED_ENTRY_ROUTINE_NAME', start)
    body = walker[start:end].replace('              case abk_routine:', '              case abk_param_ref: break;\n              case abk_routine:').replace('expect_error_str(\"unknown type in IL walk\");', '/* Retain the EDG unknown discriminator as data. */')
    # Compile the stock walker with subtree/needed mutations disabled. The
    # exported references retain both primary and secondary links, list heads,
    # next links, and EDG's exact union-arm predicates.
    macros = r'''
#undef remap_ptr
#define remap_ptr(ptr, ptr_type, kind) { reference(#ptr, (const void *)(ptr), (an_il_entry_kind)(kind)); }
#undef remap_list_ptr
#define remap_list_ptr(ptr, ptr_type, kind) { reference(#ptr, (const void *)(ptr), (an_il_entry_kind)(kind)); }
#undef walk_ptr
#define walk_ptr(ptr, ptr_type, kind) { reference(#ptr, (const void *)(ptr), (an_il_entry_kind)(kind), true); }
#undef walk_list_ptr
#define walk_list_ptr(ptr, ptr_type, kind) walk_ptr(ptr, ptr_type, kind)
#undef walk_list_on_link_field
#define walk_list_on_link_field(ptr, ptr_type, kind, link_field) walk_ptr(ptr, ptr_type, kind)
#undef remap_next_ptr
#define remap_next_ptr(ptr, ptr_type, kind) walk_ptr(ptr, ptr_type, kind)
#undef walk_next_ptr
#define walk_next_ptr(ptr, ptr_type, kind) walk_ptr(ptr, ptr_type, kind)
#undef walk_string_ptr
#define walk_string_ptr(ptr, kind, length) { string_field(#ptr, (const char *)(ptr), (an_il_entry_kind)(kind), (sizeof_t)(length)); }
#undef conditionally_clear_fe_pointer
#define conditionally_clear_fe_pointer(ptr) { observe_frontend_field(#ptr, (ptr), 0); }
#undef clear_or_walk_name_reference_field
#define clear_or_walk_name_reference_field(name_ref_ptr, field, ptr_type, entry_kind) walk_ptr(field, ptr_type, entry_kind)
'''
    # Each union arm is observed under the stock pointer-walk predicate for
    # that arm. Scalar-only arms use an explicit discriminator mapping below.
    def union_groups(body, prefix, record, inside=False):
        groups = {}
        for name, typ, suffix in fields(body):
            path = prefix + name
            if typ == 'union':
                for arm, arm_type, arm_body in fields(suffix):
                    arm_path = path + '.' + arm
                    if arm_type == 'struct':
                        groups[arm_path] = scalar_lines(arm_body, arm_path + '.', record)
                        groups.update(union_groups(arm_body, arm_path + '.', record, True))
                    elif arm_type in records and records[arm_type][0] == 'struct':
                        groups[arm_path] = scalar_lines(records[arm_type][1], arm_path + '.', record)
                        groups.update(union_groups(records[arm_type][1], arm_path + '.', record, True))
                    elif '*' not in resolve(arm_type) and not arm_type.endswith('_ptr'):
                        groups[arm_path] = ['scalar("' + arm_path + '", eptr->' + arm_path + ');']
                gaps.discard((record, path, 'active-union-selection'))
            elif typ == 'struct':
                groups.update(union_groups(suffix, path + '.', record, inside))
            elif typ in records and records[typ][0] == 'struct':
                groups.update(union_groups(records[typ][1], path + '.', record, inside))
        return groups

    manual = {
        ('a_type', 'variant.float_kind'): 'eptr->kind==tk_float || eptr->kind==tk_complex || eptr->kind==tk_imaginary',
        ('a_type', 'variant.fixed_point'): 'eptr->kind==tk_fixed_point',
        ('a_type', 'variant.array.variant.number_of_elements'): 'eptr->kind==tk_array && !eptr->variant.array.is_variable_size_array && !eptr->variant.array.is_template_dependent_size_array && !eptr->variant.array.is_vla',
        ('an_expr_node', 'variant.param_ref'): 'eptr->kind==enk_param_ref',
        ('a_constant', 'variant.integer_value'): 'eptr->kind==ck_integer',
        ('a_constant', 'variant.fixed_point_value'): 'eptr->kind==ck_fixed_point',
        ('a_constant', 'variant.float_value'): 'eptr->kind==ck_float || eptr->kind==ck_imaginary',
        ('a_routine', 'variant.opname_kind'): 'eptr->special_kind==sfk_operator',
        ('a_routine', 'variant.builtin_function_kind'): 'eptr->special_kind==sfk_none',
        ('a_routine', 'variant.ctor_dtor'): 'eptr->special_kind==sfk_constructor || eptr->special_kind==sfk_destructor',
        ('a_routine', 'number.constexpr_intrinsic'): 'eptr->is_constexpr_intrinsic',
        ('a_routine', 'number.virtual_function'): '!eptr->is_constexpr_intrinsic && eptr->is_virtual && !eptr->is_consteval',
        ('a_template_arg', 'variant.integer_value'): 'eptr->kind==tak_nontype && eptr->is_array_bound_of_unknown_type',
        ('a_subobject_path', 'variant.ptr_offset'): 'eptr->is_offset',
        ('a_constant', 'variant.address.variant.param_ref'): 'eptr->kind==ck_address && eptr->variant.address.kind==abk_param_ref',
        ('a_constant', 'variant.template_param.variant.coordinates'): 'eptr->kind==ck_template_param && eptr->variant.template_param.kind==tpck_param',
        ('a_constant', 'variant.designator.variant.array_element'): 'eptr->kind==ck_designator && !eptr->variant.designator.is_field_designator && !eptr->variant.designator.is_generic',
        ('a_variable', 'asm_name_or_reg.id'): 'eptr->has_named_register_storage_class',
        ('a_variable', 'asm_name_or_reg.reg'): '!eptr->has_named_register_storage_class && !eptr->asm_name_is_valid',
        ('a_decl_position_supplement', 'variant.declarator_range'): 'true',
        ('a_decl_position_supplement', 'variant.enum_value_range'): 'true',
        ('a_decl_position_supplement', 'variant.namespace_definition_range'): 'true',
        ('a_pragma', 'variant.stdc'): 'eptr->kind==pk_stdc',
        ('a_pragma', 'variant.upc'): 'eptr->kind==pk_upc',
        ('a_pragma', 'variant.upc.value.access_method'): 'eptr->kind==pk_upc && eptr->variant.upc.kind==upc_pk_access',
        ('a_pragma', 'variant.upc.value.operation'): 'eptr->kind==pk_upc && eptr->variant.upc.kind==upc_pk_coherence',
        ('a_pragma', 'variant.gcc'): 'eptr->kind==pk_gcc_immediate || eptr->kind==pk_gcc_next_token',
        ('a_pragma', 'variant.gcc.variant.visibility'): '(eptr->kind==pk_gcc_immediate || eptr->kind==pk_gcc_next_token) && eptr->variant.gcc.kind==gcc_pk_visibility_push',
        ('an_ms_attribute_arg', 'variant.integer_value'): 'eptr->kind==msaak_integer',
        ('an_ms_attribute_arg', 'variant.bool_value'): 'eptr->kind==msaak_boolean',
        ('an_ms_attribute_arg', 'variant.enum_value'): 'eptr->kind==msaak_enumeration',
        ('an_expr_node', 'variant.lowered_eh.variant.cleanup_region_number'): 'eptr->kind==enk_lowered_eh_construct && (eptr->variant.lowered_eh.kind==leck_cleanup_state || eptr->variant.lowered_eh.kind==leck_unreachable_cleanup_state)',


    }

    def entry(match):
        pointer, tail = match[1].strip(), match[2]
        typename = resolve(pointer)
        typename = re.sub(r'\b(struct|const)\b', '', typename).replace('*', '').strip()
        if typename not in records:
            if not re.search(r'\b' + re.escape(typename) + r'\b', configured):
                configured_disabled.add(typename)
            else:
                gaps.add((typename, '$entry', 'declaration-not-in-configured-inventory'))
            return match[0]
        top = scalar_lines(records[typename][1], '', typename)
        groups = union_groups(records[typename][1], '', typename)
        deferred = {}
        for path, lines in sorted(groups.items(), key=lambda item: len(item[0])):
            if not lines:
                continue
            predicate = manual.get((typename, path))
            if predicate:
                top.append('if (' + predicate + ') { ' + ' '.join(lines) + ' }')
            else:
                deferred[path] = lines
        used = set()
        patches = []
        for call in re.finditer(r'\b(?:walk|remap)_\w+\s*\(\s*eptr->([\w.]+)', tail):
            path = call[1]
            lines = []
            for group, values in deferred.items():
                if path == group or path.startswith(group + '.'):
                    lines.extend(values)
                    used.add(group)
            if not lines:
                continue
            begin = tail.index('(', call.start())
            depth, end = 1, begin + 1
            while depth:
                depth += (tail[end] == '(') - (tail[end] == ')')
                end += 1
            if tail[end:end+1] == ';':
                end += 1
            original = tail[call.start():end].rstrip(';')
            patches.append((call.start(), end, '{ ' + ' '.join(lines) + ' ' + original + '; }'))
        for begin, end, replacement in reversed(patches):
            tail = tail[:begin] + replacement + tail[end:]
        for path in deferred.keys() - used:
            gaps.add((typename, path, 'scalar-union-arm-not-observed-by-stock-pointer-branch'))
        return '#define eptr ((' + pointer + ')entry_ptr)\n' + '\n'.join('        ' + x for x in top) + tail + '#undef eptr'
    body = re.sub(r'#define eptr \(\(([^)]+)\)entry_ptr\)(.*?)#undef eptr', entry, body, flags=re.S)
    class_scalars = scalar_lines(records['a_class_type_supplement'][1], '', 'a_class_type_supplement')
    body = body.replace('      ) {\n        /* Fields to be processed', '      ) {\n' + '\n'.join('        ' + line.replace('eptr->', 'ctsp->') for line in class_scalars) + '\n        /* Fields to be processed')
    # The encoding generator participates in the field/configuration fingerprint.
    fingerprint = hashlib.sha256(walker.encode() + configured.encode() + Path(__file__).read_bytes()).hexdigest()
    static_coverage = []
    for record, field, detail in sorted(gaps):
        static_coverage.append('graph().coverage.emplace(0, ' + json.dumps(record + '.' + field) + ', "gap", ' + json.dumps(detail) + ');\n')
    for record in sorted(configured_disabled):
        static_coverage.append('graph().coverage.emplace(0, ' + json.dumps('$configuration.disabled.' + record) + ', "configuration", "Not declared by configured EDG preprocessing; stock branch disabled");\n')
    audited_pointer_unions = {
        ('a_class_type_supplement', 'lambda_parent'),
        ('a_constant', 'variant.aggregate.field_or_base'),
        ('a_constant', 'variant.dynamic_init.field_or_base'),
        ('a_local_static_variable_init', 'initializer'),
        ('a_name_qualifier', 'qualifier'),
        ('a_using_decl', 'qualifier'),
        ('a_variable', 'initializer'),
    }
    for record, field in sorted(pointer_unions & audited_pointer_unions):
        static_coverage.append('graph().coverage.emplace(0, ' + json.dumps(record + '.' + field) + ', "retained", "Pointer-only union: selected arm retained by stock discriminator branch");\n')
    output = ('// Generated by backend/generate_typed_graph.py; do not edit.\n'
              '// Generator, stock walker and configured declaration SHA256: ' + fingerprint + '\n'
              '#define DOT_GRAPH_FINGERPRINT \"' + fingerprint + '\"\n'
              '#define DO_SUBTREE_WALK FALSE\n#define NEEDED_FLAG_WALK FALSE\n#define KEEP_IN_IL_WALK FALSE\n'
              '#define WALK_ENTRY_ROUTINE_STATIC inline\n#define WALK_ENTRY_ROUTINE_NAME capture_entry\n'
              + walker[:start] + macros + body + '\n'
              '#define UNDEF_WALK_ENTRY_MACROS_AT_END 1\n'
              + walker[walker.index('#ifdef UNDEF_WALK_ENTRY_MACROS_AT_END'):] + '\n'
              '#undef UNDEF_WALK_ENTRY_MACROS_AT_END\n'
              + 'inline void register_generated_coverage() {\n' + ''.join(static_coverage) + '}\n'
              '#undef DO_SUBTREE_WALK\n#undef NEEDED_FLAG_WALK\n#undef KEEP_IN_IL_WALK\n'
              '#undef WALK_ENTRY_ROUTINE_STATIC\n#undef WALK_ENTRY_ROUTINE_NAME\n#undef walk_next_ptr\n')
    return output, dict(fingerprint=fingerprint, retained=sorted(retained), gaps=sorted(gaps), configured_disabled=sorted(configured_disabled), pointer_unions=sorted(pointer_unions))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--walker', type=Path, required=True)
    parser.add_argument('--configured-il', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--inventory', type=Path, required=True)
    args = parser.parse_args()
    output, coverage = generate(args.walker.read_text(), args.configured_il.read_text())
    args.output.write_text(output)
    args.inventory.write_text(json.dumps(coverage, indent=2) + '\n')


if __name__ == '__main__':
    main()
