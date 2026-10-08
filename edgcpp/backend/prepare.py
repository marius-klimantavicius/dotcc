#!/usr/bin/env python3
"""Install the version-controlled backend overlay into the pinned EDG checkout."""
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[1]
for source, target in [
    (root / "backend/dot_gen_object_macros.h", root / "upstream/src/dot_gen_object_macros.h"),
    (root / "backend/dot_gen_object_source.h", root / "upstream/src/dot_gen_object_source.h"),
    (root / "backend/dot_gen_object.h", root / "upstream/src/dot_gen_object.h"),
    (root / "backend/dot_gen_primitive_views.h", root / "upstream/src/dot_gen_primitive_views.h"),
    (root / "backend/dot_gen_variable_aliases.h", root / "upstream/src/dot_gen_variable_aliases.h"),
    (root / "backend/dot_gen_routine_symbols.h", root / "upstream/src/dot_gen_routine_symbols.h"),
    (root / "backend/dot_gen_semaphore.h", root / "upstream/src/dot_gen_semaphore.h"),
    (root / "backend/dot_gen_provider_pruning.h", root / "upstream/src/dot_gen_provider_pruning.h"),
    (root / "backend/dot_gen_posix.h", root / "upstream/src/dot_gen_posix.h"),
    (root / "backend/dot_gen_unbound.h", root / "upstream/src/dot_gen_unbound.h"),
    (root / "backend/dot_gen_source_control.h", root / "upstream/src/dot_gen_source_control.h"),
    (root / "backend/dot_gen_source_expressions.h", root / "upstream/src/dot_gen_source_expressions.h"),
    (root / "backend/dot_gen_source_expression_api.h", root / "upstream/src/dot_gen_source_expression_api.h"),
    (root / "backend/dot_gen_bitfield_properties.h", root / "upstream/src/dot_gen_bitfield_properties.h"),
    (root / "backend/dot_gen_source_names.h", root / "upstream/src/dot_gen_source_names.h"),
    (root / "backend/dot_gen_initializer_identity.h", root / "upstream/src/dot_gen_initializer_identity.h"),
    (root / "backend/dot_gen_source_storage.h", root / "upstream/src/dot_gen_source_storage.h"),
    (root / "backend/dot_gen_overflow.h", root / "upstream/src/dot_gen_overflow.h"),
    (root / "backend/dot_gen_volatile_bits.h", root / "upstream/src/dot_gen_volatile_bits.h"),
    (root / "backend/dot_gen_coroutine_provenance.h", root / "upstream/src/dot_gen_coroutine_provenance.h"),
    (root / "backend/dot_gen_coroutine_view.h", root / "upstream/src/dot_gen_coroutine_view.h"),
    (root / "backend/dot_gen_coroutine_cps.h", root / "upstream/src/dot_gen_coroutine_cps.h"),
    (root / "backend/dot_gen_process.h", root / "upstream/src/dot_gen_process.h"),
    (root / "backend/runtime/libc-process-exit-bindings.def", root / "upstream/src/libc-process-exit-bindings.def"),
    (root / "backend/runtime/libc-c11-threads-bindings.def", root / "upstream/src/libc-c11-threads-bindings.def"),
    (root / "backend/runtime/libc-assert-bindings.def", root / "upstream/src/libc-assert-bindings.def"),
    (root / "backend/dot_gen_coroutine_emit.h", root / "upstream/src/dot_gen_coroutine_emit.h"),
    (root / "backend/dot_gen_statement_expression.h", root / "upstream/src/dot_gen_statement_expression.h"),
    (root / "backend/dot_gen_c11_atomics.h", root / "upstream/src/dot_gen_c11_atomics.h"),
    (root / "backend/dot_gen_coroutine.h", root / "upstream/src/dot_gen_coroutine.h"),
    (root / "backend/dot_gen_integer_intrinsics.h", root / "upstream/src/dot_gen_integer_intrinsics.h"),
    (root / "backend/runtime/libc-timezone-bindings.def", root / "upstream/src/libc-timezone-bindings.def"),
    (root / "backend/runtime/libc-inet-bindings.def", root / "upstream/src/libc-inet-bindings.def"),
    (root / "backend/runtime/libc-wide-files-bindings.def", root / "upstream/src/libc-wide-files-bindings.def"),
    (root / "backend/dot_gen_setjmp.h", root / "upstream/src/dot_gen_setjmp.h"),
    (root / "backend/runtime/libc-environment-bindings.def", root / "upstream/src/libc-environment-bindings.def"),
    (root / "backend/runtime/libc-command-bindings.def", root / "upstream/src/libc-command-bindings.def"),
    (root / "backend/runtime/libc-signal-bindings.def", root / "upstream/src/libc-signal-bindings.def"),
    (root / "backend/dot_gen_unexpected.h", root / "upstream/src/dot_gen_unexpected.h"),
    (root / "backend/dot_gen_storage_types.h", root / "upstream/src/dot_gen_storage_types.h"),
    (root / "backend/runtime/libc-process-bindings.def", root / "upstream/src/libc-process-bindings.def"),
    (root / "backend/dot_gen_ir.h", root / "upstream/src/dot_gen_ir.h"),
    (root / "backend/dot_gen_ir_adapter.h", root / "upstream/src/dot_gen_ir_adapter.h"),
    (root / "backend/runtime/libc-calendar-bindings.def", root / "upstream/src/libc-calendar-bindings.def"),
    (root / "backend/dot_gen_terminate.h", root / "upstream/src/dot_gen_terminate.h"),
    (root / "backend/runtime/libc-cpu-clock-bindings.def", root / "upstream/src/libc-cpu-clock-bindings.def"),
    (root / "backend/runtime/libc-clock-bindings.def", root / "upstream/src/libc-clock-bindings.def"),
    (root / "backend/dot_gen_vla.h", root / "upstream/src/dot_gen_vla.h"),
    (root / "backend/runtime/libc-locale-bindings.def", root / "upstream/src/libc-locale-bindings.def"),
    (root / "backend/runtime/libc-locale-environment-bindings.def", root / "upstream/src/libc-locale-environment-bindings.def"),
    (root / "backend/dot_gen_exception_ptr.h", root / "upstream/src/dot_gen_exception_ptr.h"),
    (root / "backend/runtime/libc-numeric-bindings.def", root / "upstream/src/libc-numeric-bindings.def"),
    (root / "backend/runtime/libc-math-bindings.def", root / "upstream/src/libc-math-bindings.def"),
    (root / "backend/dot_gen_main.h", root / "upstream/src/dot_gen_main.h"),
    (root / "backend/dot_gen_threads.h", root / "upstream/src/dot_gen_threads.h"),
    (root / "backend/dot_gen_materialize.h", root / "upstream/src/dot_gen_materialize.h"),
    (root / "backend/dot_gen_members.h", root / "upstream/src/dot_gen_members.h"),
    (root / "backend/dot_gen_intrinsics.h", root / "upstream/src/dot_gen_intrinsics.h"),
    (root / "backend/dot_gen_exception_identity.h", root / "upstream/src/dot_gen_exception_identity.h"),
    (root / "backend/dot_gen_varargs.h", root / "upstream/src/dot_gen_varargs.h"),
    (root / "backend/dot_gen_atomics.h", root / "upstream/src/dot_gen_atomics.h"),
    (root / "backend/runtime/libc-files-bindings.def", root / "upstream/src/libc-files-bindings.def"),
    (root / "backend/runtime/libc-files-management-bindings.def", root / "upstream/src/libc-files-management-bindings.def"),
    (root / "backend/runtime/libc-text-bindings.def", root / "upstream/src/libc-text-bindings.def"),
    (root / "backend/runtime/libc-float-bindings.def", root / "upstream/src/libc-float-bindings.def"),
    (root / "backend/dot_sys_predef.c", root / "upstream/src/dot_sys_predef.c"),
    (root / "backend/target/portable-lp64-binary80-v1.def", root / "upstream/src/dot_gen_target_macros.def"),
    (root / "backend/target/portable-lp64-binary80-v2.def", root / "upstream/src/dot_gen_target_macros_v2.def"),
    (root / "backend/dot_gen_imports.h", root / "upstream/src/dot_gen_imports.h"),
    (root / "backend/runtime/libc-bindings.def", root / "upstream/src/libc-bindings.def"),
    (root / "backend/dot_gen_lifecycle.h", root / "upstream/src/dot_gen_lifecycle.h"),
    (root / "backend/dot_gen_typeinfo.h", root / "upstream/src/dot_gen_typeinfo.h"),
    (root / "backend/dot_gen_runtime.h", root / "upstream/src/dot_gen_runtime.h"),
    (root / "backend/dot_gen_rtti.h", root / "upstream/src/dot_gen_rtti.h"),
    (root / "backend/dot_gen_virtual.h", root / "upstream/src/dot_gen_virtual.h"),
    (root / "backend/dot_gen_exceptions.h", root / "upstream/src/dot_gen_exceptions.h"),
    (root / "backend/dot_gen_order.h", root / "upstream/src/dot_gen_order.h"),
    (root / "backend/dot_gen_sequence.h", root / "upstream/src/dot_gen_sequence.h"),
    (root / "backend/dot_gen_arrays.h", root / "upstream/src/dot_gen_arrays.h"),
    (root / "backend/dot_gen_cleanup.h", root / "upstream/src/dot_gen_cleanup.h"),
    (root / "backend/dot_gen_trace.h", root / "upstream/src/dot_gen_trace.h"),
    (root / "backend/dot_gen_byte_data.h", root / "upstream/src/dot_gen_byte_data.h"),
    (root / "backend/dot_gen_primitive_data.h", root / "upstream/src/dot_gen_primitive_data.h"),
    (root / "backend/dot_gen_record_data.h", root / "upstream/src/dot_gen_record_data.h"),
    (root / "backend/dot_gen_be.c", root / "upstream/src/dot_gen_be.c"),
    (root / "backend/cmake/CMakeLists.txt", root / "upstream/src/cmake/cpfe-dot/CMakeLists.txt"),
    (root / "backend/cmake/oracle/CMakeLists.txt", root / "upstream/src/cmake/cpfe-dot-oracle/CMakeLists.txt"),
]:
    target.parent.mkdir(parents=True, exist_ok=True)
    if not target.exists() or source.read_bytes() != target.read_bytes():
        shutil.copyfile(source, target)
cmake = root / "upstream/src/CMakeLists.txt"
for target in ("cpfe-dot", "cpfe-dot-oracle"):
    directive = f"add_subdirectory(cmake/{target})"
    if directive not in cmake.read_text():
        with cmake.open("a") as out:
            out.write("\n# Local dotnet backend overlay.\n" + directive + "\n")

# Retained lifetime IL otherwise loses destination/cursor descriptors before the
# backend runs. Preserve only attached descriptions for this target; ordinary
# EDG backends and detached/dead lifetimes keep their original freeing path.
lower = root / "upstream/src/lower_il.c"
original = """      if (dip->destructible_entity_descr != NULL) {
        free_destructible_entity_descr(dip->destructible_entity_descr);"""
patched = """      if (dip->destructible_entity_descr != NULL
#if defined(EDG_DOT_BACKEND)
          && (detach || !keep_object_lifetime_info_in_lowered_il)
#endif
         ) {
        free_destructible_entity_descr(dip->destructible_entity_descr);"""
source = lower.read_text()
if patched not in source:
    if source.count(original) != 1:
        raise RuntimeError("EDG lifetime retention overlay does not match pinned source")
    lower.write_text(source.replace(original, patched))

# Read-only source declaration/type snapshots precede incremental lowering.
# These hooks are compiled only into cpfe-dot and active only in .dotobj mode.
# The pinned replacement guards make preparation idempotent and fail closed if
# a future upstream update moves the semantic boundary.
def object_source_overlay(path, original, patched):
    source = path.read_text()
    if patched in source:
        return
    if source.count(original) != 1:
        raise RuntimeError(f"EDG object source overlay does not match pinned {path.name}")
    path.write_text(source.replace(original, patched))

# Observe accepted macro changes without depending on optional EDG IL macro
# capture. The stock decoder owns preprocessing semantics and token spelling.
for filename in ("macro.c", "preproc.c", "symbol_tbl.c"):
    object_source_overlay(root / "upstream/src" / filename,
        '#include "fe_common.h"',
        '#include "fe_common.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_macros.h"\n#endif')
macro = root / "upstream/src/macro.c"
macro_observer = r'''
#if defined(EDG_DOT_BACKEND)
static void dot_object_observe_macro(a_symbol_ptr symbol,
                                     const a_source_position &position,
                                     const char *origin)
{
  if (!dot_object_macros::enabled()) return;
  auto definition = symbol->variant.macro_def;
  // Bound the canonical-text export to replacement sections supported by this
  // profile. Inspect EDG's encoded section kinds, never matching source text:
  // a quoted "__VA_OPT__" is ordinary text and remains supported. Optional
  // replacement groups need a richer representation before we can export them.
  for (char *cursor = definition->repl_text; cursor && *cursor != (int)rt_end;) {
    a_repl_text_seq_kind kind = (a_repl_text_seq_kind)*(cursor++);
    sizeof_t number;
    get_macro_repl_text_number(number, cursor);
    switch (kind) {
      case rt_text:
        cursor += number;
        break;
      case rt_optional_text:
        dot_object_macros::reject_feature("__VA_OPT__ macro replacement groups");
        return;
      case rt_stringized_raw_argument:
      case rt_charized_raw_argument:
        if (number == MAX_REPL_TEXT_NUMBER) {
          dot_object_macros::reject_feature("__VA_OPT__ macro replacement groups");
          return;
        }
        break;
      case rt_raw_argument:
      case rt_microsoft_maybe_raw_argument:
      case rt_paste:
      case rt_argument:
      case rt_microsoft_magic_arg_marker:
        break;
      default:
        dot_object_macros::reject_feature("unknown macro replacement section");
        return;
    }
  }
  // Stock EDG guard restores the exact prior scratch pointer, capacity and
  // cursor, including any live pointers into that buffer.
  a_temp_text_buffer_swap scratch;
  make_definition_string(symbol);
  dot_object_macros::Definition value;
  value.name = symbol->header->identifier;
  value.text.assign(temp_text_buffer, pos_in_temp_text_buffer - 1);
  value.origin = origin;
  value.position = position;
  value.object_like = definition->object_like;
  value.variadic = definition->variadic;
  value.dynamic = definition->repl_text == NULL;
  for (auto parameter = definition->param_list; parameter; parameter = parameter->next)
    value.parameters.emplace_back(parameter->name);
  dot_object_macros::define(value);
}
#endif
'''
# Upgrade an already prepared observer in place. Anchoring only the preceding
# upstream function would append a second helper after a local overlay edit.
observer_start = '\n#if defined(EDG_DOT_BACKEND)\nstatic void dot_object_observe_macro('
macro_source = macro.read_text()
if observer_start in macro_source:
    if macro_source.count(observer_start) != 1:
        raise RuntimeError("Duplicate EDG macro object observer")
    start = macro_source.index(observer_start)
    end = macro_source.index('}\n#endif\n', start) + len('}\n#endif\n')
    if macro_source[start:end] != macro_observer:
        macro.write_text(macro_source[:start] + macro_observer + macro_source[end:])
else:
    object_source_overlay(macro,
        '}  /* make_definition_string */',
        '}  /* make_definition_string */\n' + macro_observer)
object_source_overlay(macro,
    'def_done:;\n    if (assoc_symbol != NULL && !discard_new_definition) {',
    'def_done:;\n    if (assoc_symbol != NULL && !discard_new_definition) {\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_observe_macro(assoc_symbol, start_pos, processing_predefined_macro ? "predefined" :\n'
    '          curr_cmd_line_or_predef_macro_def != NULL ? "command-line" : "source");\n'
    '#endif')
object_source_overlay(macro,
    '    mdp->repl_text   = repl_text;\n#if RECORD_MACROS_IN_IL',
    '    mdp->repl_text   = repl_text;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_observe_macro(sym_ptr, null_source_position, "predefined");\n'
    '#endif\n#if RECORD_MACROS_IN_IL')
object_source_overlay(macro,
    '      if (err && !suppress_error) {\n        str_command_line_error(ec_cl_invalid_macro_undefinition, du_str);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      if (!err) dot_object_macros::undef(du_str, strlen(du_str), null_source_position, "command-line");\n'
    '#endif\n'
    '      if (err && !suppress_error) {\n        str_command_line_error(ec_cl_invalid_macro_undefinition, du_str);')
object_source_overlay(macro,
    '  if (scanning_module_macro) {\n',
    '  if (scanning_module_macro) {\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_macros::reject_feature("module macro imports");\n'
    '#endif\n')
object_source_overlay(macro,
    '  /* Reset the replacement text for the __DATE__ and __TIME__ macro symbols. */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_macros::reject_feature("precompiled header macro imports");\n'
    '#endif\n'
    '  /* Reset the replacement text for the __DATE__ and __TIME__ macro symbols. */')
preproc = root / "upstream/src/preproc.c"
object_source_overlay(preproc,
    '      /* No such macro, so #undef is ignored. */',
    '      /* No such macro, so #undef is ignored. */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_macros::undef(id_ptr, id_len, pos_curr_token, "source");\n'
    '#endif')
object_source_overlay(preproc,
    '      /* Remove the macro\'s definition.  The a_macro_def entry pointed to\n'
    '         by the symbol is not freed, and is therefore just lost.  */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_macros::undef(id_ptr, id_len, pos_curr_token, "source");\n'
    '#endif\n'
    '      /* Remove the macro\'s definition.  The a_macro_def entry pointed to\n'
    '         by the symbol is not freed, and is therefore just lost.  */')
symbols = root / "upstream/src/symbol_tbl.c"
for kind in ("push", "pop"):
    # Anchor the final assignment in each function, so ordinary pragma parsing
    # remains untouched and an inactive conditional branch never triggers it.
    ending = '}  /* ' + kind + '_macro_pragma */'
    object_source_overlay(symbols, ending,
        '#if defined(EDG_DOT_BACKEND)\n'
        '  dot_object_macros::reject_feature("' + kind + '_macro pragma");\n'
        '#endif\n' + ending)

for filename in ("lower_il.c", "lower_c99.c"):
    object_source_overlay(root / "upstream/src" / filename,
        '#include "exprutil.h"',
        '#include "exprutil.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
object_source_overlay(lower,
    '  a_type_ptr ptr_return_type, new_type, member_type;\n',
    '  a_type_ptr ptr_return_type, new_type, member_type;\n'
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_type(type);\n#endif\n')
object_source_overlay(lower,
    '  db_enter(1, "lower_il_memory_region");',
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_scope(scope);\n#endif\n'
    '  db_enter(1, "lower_il_memory_region");')
object_source_overlay(lower,
    '  if (!visited_yet(routine)) {\n    check_assertion(!ignore_routine_in_back_end(routine));',
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_routine(routine);\n#endif\n'
    '  if (!visited_yet(routine)) {\n    check_assertion(!ignore_routine_in_back_end(routine));')
c99 = root / "upstream/src/lower_c99.c"
object_source_overlay(c99,
    '  il_lowering_underway = TRUE;\n  curr_context = NULL;',
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_scope(scope);\n#endif\n'
    '  il_lowering_underway = TRUE;\n  curr_context = NULL;')
