#!/usr/bin/env python3
"""Install the version-controlled backend overlay into the pinned EDG checkout."""
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[1]
for source, target in [
    (root / "backend/dot_gen_semantic.h", root / "upstream/src/dot_gen_semantic.h"),
    (root / "backend/dot_gen_semantic_sqlite.h", root / "upstream/src/dot_gen_semantic_sqlite.h"),
    (root / "backend/dot_gen_object_field_state.h", root / "upstream/src/dot_gen_object_field_state.h"),
    (root / "backend/dot_gen_object_graph.h", root / "upstream/src/dot_gen_object_graph.h"),
    (root / "backend/dot_gen_object_graph_walk.h", root / "upstream/src/dot_gen_object_graph_walk.h"),
    (root / "backend/dot_gen_object_macros.h", root / "upstream/src/dot_gen_object_macros.h"),
    (root / "backend/dot_gen_object_source.h", root / "upstream/src/dot_gen_object_source.h"),
    (root / "backend/dot_gen_object_source_tls.h", root / "upstream/src/dot_gen_object_source_tls.h"),
    (root / "backend/dot_gen_object_tls_writer.h", root / "upstream/src/dot_gen_object_tls_writer.h"),
    (root / "backend/dot_gen_object_source_operations.h", root / "upstream/src/dot_gen_object_source_operations.h"),
    (root / "backend/dot_gen_object_source_constraints.h", root / "upstream/src/dot_gen_object_source_constraints.h"),
    (root / "backend/dot_gen_object_source_namespaces.h", root / "upstream/src/dot_gen_object_source_namespaces.h"),
    (root / "backend/dot_gen_object_namespaces.h", root / "upstream/src/dot_gen_object_namespaces.h"),
    (root / "backend/dot_gen_object_constraints_writer.h", root / "upstream/src/dot_gen_object_constraints_writer.h"),
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

# Observe availability of the one pinned variable bit not initialized by
# clear_variable. These hooks do not write EDG state or infer its value.
for filename in ("il_alloc.c", "decl_inits.c", "trans_copy.c"):
    object_source_overlay(root / "upstream/src" / filename,
        '#include "fe_common.h"',
        '#include "fe_common.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_field_state.h"\n#endif')
object_source_overlay(root / "upstream/src/il_alloc.c",
    '}  /* clear_variable */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::field_observation::variable_cleared(vp);\n'
    '#endif\n}  /* clear_variable */')
object_source_overlay(root / "upstream/src/decl_inits.c",
    '    if (!def_init_performed) var->uninitialized = TRUE;',
    '    if (!def_init_performed) var->uninitialized = TRUE;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    if (!def_init_performed)\n'
    '      dot_object_source::field_observation::variable_uninitialized_assigned(var);\n'
    '#endif')
object_source_overlay(root / "upstream/src/trans_copy.c",
    '    (void)memcpy(copy, ptr, size_t_arg(sizeof_il_entry[(int)kind]));',
    '    (void)memcpy(copy, ptr, size_t_arg(sizeof_il_entry[(int)kind]));\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    if (kind == iek_variable)\n'
    '      dot_object_source::field_observation::variable_copied(ptr, copy);\n'
    '#endif')

# The selected routine exists before the declaration's symbol/type does.
# Retain that stock factory edge for nested anonymous declarators only.
object_source_overlay(root / "upstream/src/templates.c",
    '#include "fe_common.h"',
    '#include "fe_common.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
object_source_overlay(root / "upstream/src/templates.c",
    '  rp = alloc_routine();\n  {\n    /* Create a routine type by rescanning the original declaration',
    '  rp = alloc_routine();\n#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::RoutineInstantiationCapture dot_routine_instantiation_capture(\n'
    '    rp, tssp->variant.function.decl_cache->decl_info->template_decl, templ_arg_list);\n'
    '#endif\n  {\n    /* Create a routine type by rescanning the original declaration')

# Preserve original enclosing template context of reparsed routine declarators.
object_source_overlay(root / "upstream/src/declarator.c",
    '#include "fe_common.h"',
    '#include "fe_common.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
object_source_overlay(root / "upstream/src/declarator.c",
    '  db_exit();\n}  /* function_declarator */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_routine_declarator_context(*new_type_ptr, start_pos);\n'
    '#endif\n  db_exit();\n}  /* function_declarator */')

# Observe the precise lazy defaulted/generated exception-specification boundary.
class_declarations = root / "upstream/src/class_decl.c"
object_source_overlay(class_declarations,
    '#include "fe_common.h"',
    '#include "fe_common.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
object_source_overlay(class_declarations,
    '  check_assertion(!C_mode() && exceptions_enabled);\n  check_assertion(rtsp->exception_specification == NULL);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::GeneratedExceptionSpecificationCapture dot_exception_specification_capture(rp);\n'
    '#endif\n'
    '  check_assertion(!C_mode() && exceptions_enabled);\n  check_assertion(rtsp->exception_specification == NULL);')
object_source_overlay(class_declarations,
    '}  /* form_exception_specification_for_generated_function */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_exception_specification_capture.complete();\n'
    '#endif\n}  /* form_exception_specification_for_generated_function */')

# Preserve the original lambda call-operator recipe before the stock factory
# clears receiver association and installs the conversion-target/static types.
object_source_overlay(class_declarations,
    '  rtsp = call_type->variant.routine.extra_info;',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_lambda_conversion_prototype(call_type, call_op);\n'
    '#endif\n'
    '  rtsp = call_type->variant.routine.extra_info;')

# Observe accepted layout inputs/results, without rerunning frontend layout.
layout_source = root / "upstream/src/layout.c"
object_source_overlay(layout_source,
    '#include "fe_common.h"',
    '#include "fe_common.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
object_source_overlay(layout_source,
    '  db_enter(3, "do_class_layout");',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_class_layout_directive(class_type);\n'
    '#endif\n  db_enter(3, "do_class_layout");')
object_source_overlay(layout_source,
    '  db_enter(4, "set_field_size_and_offset");',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_field_layout_directive(field);\n'
    '#endif\n  db_enter(4, "set_field_size_and_offset");')
object_source_overlay(layout_source,
    '  /* If the alignment of this field was explicitly specified, honor that. */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_field_layout_directive(field);\n'
    '#endif\n  /* If the alignment of this field was explicitly specified, honor that. */')
object_source_overlay(layout_source,
    '  return field_alignment;\n}  /* alignment_of_field_full */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_field_layout_alignment(field, for_alignof, field_alignment);\n'
    '#endif\n  return field_alignment;\n}  /* alignment_of_field_full */')

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
# Upgrade the former fail-closed push/pop hooks. Observations occur only after
# the stock frontend accepted and performed the actual stack operation.
for kind in ("push", "pop"):
    obsolete = ('#if defined(EDG_DOT_BACKEND)\n'
        '  dot_object_macros::reject_feature("' + kind + '_macro pragma");\n'
        '#endif\n')
    source = symbols.read_text()
    if obsolete in source:
        symbols.write_text(source.replace(obsolete, ""))
object_source_overlay(symbols,
    '    sym_hdr->saved_macro_stack = smsp;\n',
    '    sym_hdr->saved_macro_stack = smsp;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_macros::push(sym_hdr->identifier, name_pos);\n'
    '#endif\n')
object_source_overlay(symbols,
    '      free_saved_macro_state(smsp);\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_macros::pop(sym_hdr->identifier, name_pos);\n'
    '#endif\n'
    '      free_saved_macro_state(smsp);\n')

# Complete the semantic exception policy of actually referenced in-class
# defaulted members while the frontend still owns their original symbols.
# This is a query through EDG's supported API, not an edit to its IL rules.
reference_il = root / "upstream/src/il.c"
reference_policy_query = ('#if defined(EDG_DOT_BACKEND)\n'
    '  if (dot_object_source::request_referenced_defaulted_exception_policy(routine, elided_reference)) {\n'
    '    complete_defaulted_exc_spec(routine);\n'
    '  }\n'
    '#endif\n')
# Migrate the earlier end-of-function hook: force_definition can already lower
# the selected member's body and receiver before returning. Query before that
# dependency, preserving the original-type and frontend-context guards.
reference_source = reference_il.read_text()
reference_source = reference_source.replace(
    reference_policy_query + '}  /* mark_routine_referenced_full */',
    '}  /* mark_routine_referenced_full */')
reference_il.write_text(reference_source)
object_source_overlay(reference_il,
    '  force_definition_of_compiler_generated_routine(routine);',
    reference_policy_query + '  force_definition_of_compiler_generated_routine(routine);')

wrapup = root / "upstream/src/fe_wrapup.c"
object_source_overlay(wrapup,
    '#include "fe_wrapup.h"',
    '#include "fe_wrapup.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
object_source_overlay(wrapup,
    '  wrap_up_file_scopes();\n\n  in_front_end = FALSE;',
    '  wrap_up_file_scopes();\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_frontend_identity_boundary();\n'
    '#endif\n\n  in_front_end = FALSE;')

expr_source = root / "upstream/src/expr.c"
object_source_overlay(expr_source,
    '#include "statements.h"',
    '#include "statements.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
object_source_overlay(expr_source,
    '  record_typed_operator_position_in_expr_rescan_info(new_node,',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  if (!nps->needs_initialization)\n'
    '    dot_object_source::observe_new_without_initialization(ndsp, nps->dyn_init_to_free_storage,\n'
    '                                                        nps->type_position);\n'
    '#endif\n'
    '  record_typed_operator_position_in_expr_rescan_info(new_node,')

object_source_overlay(expr_source,
    '  ndsp->routine = nps->new_routine;',
    '  ndsp->routine = nps->new_routine;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_source_new_lookup(ndsp, nps->unknown_dependent_new);\n'
    '#endif')
object_source_overlay(root / "upstream/src/il.c",
    '    expr_copy->variant.new_delete = copy_new_delete;',
    '    expr_copy->variant.new_delete = copy_new_delete;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_source_new_lookup_copy(expr->variant.new_delete, copy_new_delete);\n'
    '#endif')

for filename in ("lower_il.c", "lower_c99.c", "lower_init.c"):
    object_source_overlay(root / "upstream/src" / filename,
        '#include "exprutil.h"',
        '#include "exprutil.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
init_lower = root / "upstream/src/lower_init.c"
object_source_overlay(init_lower,
    '      default_arg_list = copy_list_of_expr_trees(default_arg_list, opts);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_default_argument_copy_begin(new_routine, new_routine_scope, init_expr_lifetime);\n'
    '#endif\n'
    '      default_arg_list = copy_list_of_expr_trees(default_arg_list, opts);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_default_argument_copy_end(new_routine);\n'
    '#endif')
# Migrate the older lambda-only factory hook without duplicating observers.
old_forwarding_hook = ('      call_node = make_call_node(routine, first_arg);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_lambda_entry_body(new_routine, new_routine_scope, call_node);\n'
    '#endif')
forwarding_source = init_lower.read_text()
if old_forwarding_hook in forwarding_source:
    init_lower.write_text(forwarding_source.replace(old_forwarding_hook,
        '      call_node = make_call_node(routine, first_arg);'))
object_source_overlay(init_lower,
    '      call_node = make_call_node(routine, first_arg);',
    '      call_node = make_call_node(routine, first_arg);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_lambda_entry_body(new_routine, new_routine_scope, call_node);\n'
    '      dot_object_source::observe_default_argument_forwarding(new_routine, new_routine_scope, call_node);\n'
    '#endif')
object_source_overlay(init_lower,
    '    define_default_version_of_routine(routine, new_routine, \n'
    '                                      default_arg_list);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_default_adapter(routine, new_routine,\n'
    '        any_implied_args, cctor_case, default_arg_list != NULL);\n'
    '#endif\n'
    '    define_default_version_of_routine(routine, new_routine, \n'
    '                                      default_arg_list);')
object_source_overlay(init_lower,
    '    }  /* if */\n  }  /* if */\n  return *routine;\n}  /* make_prototyped_runtime_routine */',
    '    }  /* if */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_runtime_declaration(*routine);\n'
    '#endif\n'
    '  }  /* if */\n  return *routine;\n}  /* make_prototyped_runtime_routine */')
object_source_overlay(init_lower,
    '  ptr_elem_type = make_pointer_type(elem_type);\n',
    '  ptr_elem_type = make_pointer_type(elem_type);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::EntityInitializerCapture dot_entity_initializer_capture(\n'
    '    ndsp->freeing_of_storage_on_exception, elem_type, innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n')
for runtime_callback in ("ctor", "dtor", "cctor"):
    object_source_overlay(init_lower,
        f'    {runtime_callback}_ptr_type = make_pointer_type(function_type);',
        f'    {runtime_callback}_ptr_type = make_pointer_type(function_type);\n'
        '#if defined(EDG_DOT_BACKEND)\n'
        '    dot_object_source::observe_runtime_signature(function_type);\n'
        '#endif')
for runtime_callback in ("new_routine", "delete_routine", "two_operand_delete_routine"):
    object_source_overlay(init_lower,
        f'    {runtime_callback}_ptr_type = make_pointer_type(function_type);',
        f'    {runtime_callback}_ptr_type = make_pointer_type(function_type);\n'
        '#if defined(EDG_DOT_BACKEND)\n'
        '    dot_object_source::observe_runtime_signature(function_type);\n'
        '#endif')
object_source_overlay(init_lower,
    '    cxa_dtor_type = make_pointer_type(function_type);',
    '    cxa_dtor_type = make_pointer_type(function_type);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_runtime_signature(function_type);\n'
    '#endif')
object_source_overlay(init_lower,
    '  /* Generate the code for the destruction. */\n'
    '  add_destructor_call(dip->destructor, ipdp, /*have_complete_object=*/TRUE,',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_static_destructor_begin(routine, scope, dip, ipdp);\n'
    '  dot_object_source::NormalDestructionCapture dot_static_destruction_capture(dip, routine);\n'
    '#endif\n'
    '  /* Generate the code for the destruction. */\n'
    '  add_destructor_call(dip->destructor, ipdp, /*have_complete_object=*/TRUE,')
object_source_overlay(init_lower,
    '  ipdp->variable->referenced_non_locally = TRUE;\n'
    '  pop_generated_routine_context(scope, region_number, &grcontext);',
    '  ipdp->variable->referenced_non_locally = TRUE;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_static_destructor_complete(routine, scope);\n'
    '#endif\n'
    '  pop_generated_routine_context(scope, region_number, &grcontext);')
object_source_overlay(init_lower,
    '    /* There\'s no need to call the destructor; skip the destruction processing\n'
    '       altogether. */\n'
    '    goto remove_from_list;',
    '    /* There\'s no need to call the destructor; skip the destruction processing\n'
    '       altogether. */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_static_no_effect(dip);\n'
    '#endif\n'
    '    goto remove_from_list;')
object_source_overlay(init_lower,
    '        /* This destruction has no effect and can be removed. */\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::observe_automatic_no_effect(dip);\n'
    '#endif\n'
    '        /* This destruction has no effect and can be removed. */\n')
object_source_overlay(init_lower,
    '      /* This destruction isn\'t on a constructor_inits list and isn\'t\n'
    '         needed so remove it from the destruction list (it will still\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_automatic_no_effect(dip);\n'
    '#endif\n'
    '      /* This destruction isn\'t on a constructor_inits list and isn\'t\n'
    '         needed so remove it from the destruction list (it will still\n')
object_source_overlay(init_lower,
    '        /* If an operator delete call was needed for this new call, it is\n'
    '           not needed any longer. */\n'
    '        remove_from_destruction_list(ndsp->freeing_of_storage_on_exception);',
    '        /* If an operator delete call was needed for this new call, it is\n'
    '           not needed any longer. */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::observe_automatic_no_effect(ndsp->freeing_of_storage_on_exception);\n'
    '#endif\n'
    '        remove_from_destruction_list(ndsp->freeing_of_storage_on_exception);')
object_source_overlay(init_lower,
    '  if (olp != NULL) {\n'
    "    /* If there's an object lifetime, save the destruction list and remove\n",
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::FieldInitializerCapture dot_field_initializer_capture(scope);\n'
    '#endif\n'
    '  if (olp != NULL) {\n'
    "    /* If there's an object lifetime, save the destruction list and remove\n")
object_source_overlay(init_lower,
    '        /* Neither the construction nor destruction have any effect;\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::observe_field_initializer_selection(scope, ctor_init, true);\n'
    '#endif\n'
    '        /* Neither the construction nor destruction have any effect;\n')
object_source_overlay(init_lower,
    '        dip = copy_dynamic_init(field_dip, CE_COPYING_DMI_DIP);',
    '        dip = copy_dynamic_init(field_dip, CE_COPYING_DMI_DIP);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::observe_field_initializer_selection(scope, ctor_init, false);\n'
    '#endif')
# Upgrade the earlier allocation-only observation without stacking contexts.
obsolete_array_capture = ('#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::ArrayAllocationCapture dot_array_allocation_capture(\n'
    '      ndsp->freeing_of_storage_on_exception, elem_type, innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n')
source = init_lower.read_text()
if obsolete_array_capture in source:
    init_lower.write_text(source.replace(obsolete_array_capture, ''))
object_source_overlay(init_lower,
    '    /* Construct the call of __vec_new or __array_new. */\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::ArrayAllocationCapture dot_array_allocation_capture(\n'
    '      ndsp->freeing_of_storage_on_exception, dtor_routine != NULL ? elem_dynamic_init(dip) : NULL,\n'
    '      elem_type, innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n'
    '    /* Construct the call of __vec_new or __array_new. */\n')
object_source_overlay(init_lower,
    '  /* Lower "arg". */\n  lower_expr(ptr_node);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::ArrayDeletionCapture dot_array_deletion_capture(\n'
    '    dip != NULL ? elem_dynamic_init(dip) : NULL, delete_routine,\n'
    '    new_delete_base_type_from_operation_type(ndsp->type), innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n'
    '  /* Lower "arg". */\n  lower_expr(ptr_node);')
object_source_overlay(init_lower,
    '  return call_node;\n}  /* make_vec_delete_call */',
    '#if defined(EDG_DOT_BACKEND) && IA64_ABI\n'
    '  dot_object_source::observe_array_deletion_call(call_node);\n'
    '#endif\n'
    '  return call_node;\n}  /* make_vec_delete_call */')
object_source_overlay(init_lower,
    '  return call_node;\n}  /* make_vec_new_call */',
    '#if defined(EDG_DOT_BACKEND) && IA64_ABI\n'
    '  dot_object_source::observe_array_allocation_call(call_node, entity_node, ctor_routine, dtor_routine,\n'
    '    new_routine, delete_routine, padding_size_node);\n'
    '#endif\n'
    '  return call_node;\n}  /* make_vec_new_call */')
object_source_overlay(init_lower,
    '      if (alloc_expr == NULL && init_expr == NULL) {\n'
    '        /* If neither allocation nor initialization generated any code,\n',
    '      if (alloc_expr == NULL && init_expr == NULL) {\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::observe_empty_allocation_initialization(dyn_init_to_free_storage,\n'
    '          innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n'
    '        /* If neither allocation nor initialization generated any code,\n')
object_source_overlay(init_lower,
    '  /* Make a statement containing the call and insert it at the right\n'
    '     location. */\n'
    '  (void)insert_expr_statement_set_pos(call_node, insert_location);',
    '#if defined(EDG_DOT_BACKEND) && IA64_ABI\n'
    '  dot_object_source::observe_static_registration(dip, ipdp, call_node, dtor_routine,\n'
    '    innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n'
    '  /* Make a statement containing the call and insert it at the right\n'
    '     location. */\n'
    '  (void)insert_expr_statement_set_pos(call_node, insert_location);')
object_source_overlay(init_lower,
    '        dip->destructible_entity_descr->\n'
    '                            is_destruction_for_partial_static_aggregate = TRUE;\n'
    '      } else {\n'
    '        /* Update the cleanup information so that this entity will be',
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::observe_static_partial_terminal(dip);\n'
    '#endif\n'
    '        dip->destructible_entity_descr->\n'
    '                            is_destruction_for_partial_static_aggregate = TRUE;\n'
    '      } else {\n'
    '        /* Update the cleanup information so that this entity will be')
object_source_overlay(init_lower,
    '    /* The original lifetime won\'t be used, so unbind it. */\n'
    '    unbind_object_lifetime(*init_expr_lifetime);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_initializer_lifetime_copy(*init_expr_lifetime, curr_object_lifetime,\n'
    '      innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n'
    '    /* The original lifetime won\'t be used, so unbind it. */\n'
    '    unbind_object_lifetime(*init_expr_lifetime);')
# Observe stock file-scope dynamic initialization creation and insertion.
# These hooks retain provenance; they do not change any frontend IL entry.
object_source_overlay(init_lower,
    '  a_scope_ptr file_scope = il_header.primary_scope;\n\n#if IMPLEMENTATION_SUPPORTS_MULTIPLE_THREADS',
    '  a_scope_ptr file_scope = il_header.primary_scope;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_file_initializers();\n'
    '#endif\n\n#if IMPLEMENTATION_SUPPORTS_MULTIPLE_THREADS')
object_source_overlay(init_lower,
    '  return scope;\n}  /* file_scope_init_insert_location */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_generated_initializer(scope, needed_bit_number,\n'
    '      init_priority, do_thread_local);\n'
    '#endif\n'
    '  return scope;\n}  /* file_scope_init_insert_location */')
object_source_overlay(init_lower,
    '      if (var->is_inline\n#if TEMPLATE_STATIC_DATA_MEMBER_INIT_GUARD_CODE',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_initializer_begin(scope, dip,\n'
    '          insert_location.kind == ilk_block_start,\n'
    '          insert_location.kind == ilk_after_statement,\n'
    '          insert_location.variant.statement.stmt);\n'
    '#endif\n'
    '      if (var->is_inline\n#if TEMPLATE_STATIC_DATA_MEMBER_INIT_GUARD_CODE')
object_source_overlay(init_lower,
    '#endif /* IA64_ABI */\n#if SEPARATE_ROUTINES_FOR_FILE_SCOPE_DYNAMIC_INITS\n      if (do_single_init) {',
    '#endif /* IA64_ABI */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_initializer_end(scope, dip,\n'
    '          insert_location.kind == ilk_block_start,\n'
    '          insert_location.kind == ilk_after_statement,\n'
    '          insert_location.variant.statement.stmt);\n'
    '#endif\n'
    '#if SEPARATE_ROUTINES_FOR_FILE_SCOPE_DYNAMIC_INITS\n      if (do_single_init) {')
object_source_overlay(init_lower,
    '    pop_generated_routine_context(scope, region_number, &grcontext);\n    processing_file_scope_init_routine = FALSE;',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_generated_initializer_complete(scope);\n'
    '#endif\n'
    '    pop_generated_routine_context(scope, region_number, &grcontext);\n    processing_file_scope_init_routine = FALSE;')
eh_lower = root / "upstream/src/lower_eh.c"
object_source_overlay(eh_lower,
    '  routine = scope->variant.routine.ptr;\n#if DO_FULL_PORTABLE_EH_LOWERING',
    '  routine = scope->variant.routine.ptr;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_dynamic_exception_boundary(routine);\n'
    '#endif\n#if DO_FULL_PORTABLE_EH_LOWERING')
object_source_overlay(eh_lower,
    '#include "lower_hdrs.h"',
    '#include "lower_hdrs.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
# Observe the exact memory-operand shape before stock VLA/lvalue lowering,
# then read its saved total after lowering. These hooks do not mutate EDG IL.
object_source_overlay(lower,
    '  an_asm_operand_ptr aop;\n  for (aop = aep->operands; aop != NULL; aop = aop->next) {',
    '  an_asm_operand_ptr aop;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_asm_before_lowering(aep);\n'
    '#endif\n'
    '  for (aop = aep->operands; aop != NULL; aop = aop->next) {')
object_source_overlay(lower,
    '    lower_full_expr(expr, (a_statement_ptr)NULL);\n'
    '  }  /* for */\n#endif /* GNU_EXTENSIONS_ALLOWED */\n}  /* lower_asm_statement */',
    '    lower_full_expr(expr, (a_statement_ptr)NULL);\n'
    '  }  /* for */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_asm_after_lowering(aep);\n'
    '#endif\n'
    '#endif /* GNU_EXTENSIONS_ALLOWED */\n}  /* lower_asm_statement */')
object_source_overlay(lower,
    '  a_type_ptr ptr_return_type, new_type, member_type;\n',
    '  a_type_ptr ptr_return_type, new_type, member_type;\n'
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_type(type);\n#endif\n')
object_source_overlay(lower,
    '  }  /* if */\n}  /* lower_type */',
    '  }  /* if */\n'
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_lowered_type(type);\n#endif\n'
    '}  /* lower_type */')
object_source_overlay(lower,
    '    finish_class_type(mptr_type);\n',
    '    finish_class_type(mptr_type);\n'
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_member_pointer_record(mptr_type);\n#endif\n')
object_source_overlay(lower,
    '  return vptp_type;\n}  /* make_vptp_type */',
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_erased_code_pointer(vptp_type);\n#endif\n'
    '  return vptp_type;\n}  /* make_vptp_type */')
object_source_overlay(lower,
    '          set_lowered_routine_calling_method_flag(type);\n'
    '          if (rtsp->value_returned_as_parameter) {',
    '          set_lowered_routine_calling_method_flag(type);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '          dot_object_source::observe_routine_abi(type);\n'
    '#endif\n'
    '          if (rtsp->value_returned_as_parameter) {')
object_source_overlay(lower,
    '      return_value_pointer_variable->next = scope->variant.routine.parameters;',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_result_parameter(routine, return_value_pointer_variable);\n'
    '#endif\n'
    '      return_value_pointer_variable->next = scope->variant.routine.parameters;')
object_source_overlay(lower,
    '  if (!class_has_been_prelowered(class_type)) {\n',
    '  if (!class_has_been_prelowered(class_type)) {\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_record_before_prelowering(class_type);\n'
    '#endif\n')
object_source_overlay(lower,
    '  add_scope_to_class_type(type);\n  return type;\n}  /* make_lowered_class_type */',
    '  add_scope_to_class_type(type);\n'
    '#if defined(EDG_DOT_BACKEND)\n  dot_object_source::observe_abi_record(type);\n#endif\n'
    '  return type;\n}  /* make_lowered_class_type */')
object_source_overlay(lower,
    '  *last_field = field_ptr;\n}  /* copy_field */',
    '  *last_field = field_ptr;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_field_copy(old_field_ptr, field_ptr);\n'
    '#endif\n}  /* copy_field */')
object_source_overlay(lower,
    '  (void)add_field(name_ptr, field_type, field_offset, struct_type,\n'
    '                  /*for_base_class=*/FALSE);\n}  /* add_dummy_field */',
    '  a_field_ptr observed_field = add_field(name_ptr, field_type, field_offset, struct_type,\n'
    '                  /*for_base_class=*/FALSE);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_vptr_field(observed_field);\n'
    '#else\n  (void)observed_field;\n#endif\n}  /* add_dummy_field */')
for vtt_kind in ('constructor', 'destructor'):
    object_source_overlay(lower,
        '#endif /* IA64_ABI */\n}  /* lower_' + vtt_kind + '_routine_type */',
        '#endif /* IA64_ABI */\n#if defined(EDG_DOT_BACKEND) && IA64_ABI\n'
        '  dot_object_source::observe_vtt_type(routine_type);\n#endif\n'
        '}  /* lower_' + vtt_kind + '_routine_type */')
object_source_overlay(lower,
    '    prev_param_var->next = vtt_param_var;\n',
    '    prev_param_var->next = vtt_param_var;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_vtt_parameter(ctor_routine, vtt_param_var);\n#endif\n')
object_source_overlay(lower,
    '      this_param_var->next = vtt_param_var;\n',
    '      this_param_var->next = vtt_param_var;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_vtt_parameter(dtor_routine, vtt_param_var);\n#endif\n')

# Preserve exact base-descriptor correspondence at the physical insertion.
object_source_overlay(lower,
    '  mangle_wrapper_name(entry_routine);\n',
    '  mangle_wrapper_name(entry_routine);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_thunk_wrapper(entry_routine);\n#endif\n')
object_source_overlay(init_lower,
    '  }  /* if */\n#endif /* MAINTAIN_NEEDED_FLAGS */\n}  /* add_body_for_wrapper_routine */',
    '  }  /* if */\n#endif /* MAINTAIN_NEEDED_FLAGS */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_thunk_body(routine, scope, roof_expr);\n#endif\n'
    '}  /* add_body_for_wrapper_routine */')
object_source_overlay(lower,
    '                                       a_type_ptr    struct_type)\n/*\nMake a dummy field for a base class',
    '                                       a_type_ptr    struct_type,\n'
    '                                       a_base_class_ptr observed_base)\n/*\nMake a dummy field for a base class')
object_source_overlay(lower,
    '  field_ptr->is_lowered_base_class = TRUE;\n}  /* add_base_class_dummy_field */',
    '  field_ptr->is_lowered_base_class = TRUE;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_base_field(field_ptr, observed_base);\n'
    '#else\n  (void)observed_base;\n#endif\n}  /* add_base_class_dummy_field */')
for before, after in (
    ('base_class_type, bcp->offset, \n                                 class_type);', 'base_class_type, bcp->offset, \n                                 class_type, bcp);'),
    ('base_class_type, bcp->offset,\n                                     class_type);', 'base_class_type, bcp->offset,\n                                     class_type, bcp);'),
    ('bcp->pointer_offset, class_type);', 'bcp->pointer_offset, class_type, bcp);'),
    ('base_class_type, bcp->offset,\n                                       class_type);', 'base_class_type, bcp->offset,\n                                       class_type, bcp);')):
    object_source_overlay(lower, before, after)
object_source_overlay(lower,
    '  ctsp->subobject_partner = subobject_type;\n',
    '  ctsp->subobject_partner = subobject_type;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_subobject_type(class_type, subobject_type);\n#endif\n')
object_source_overlay(eh_lower,
    '    finish_class_type(base_class_spec_type);\n',
    '    finish_class_type(base_class_spec_type);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_typeinfo_base_record(base_class_spec_type);\n'
    '#endif\n')
object_source_overlay(eh_lower,
    '    finish_class_type(typeinfo_types[(int)tik_user]);\n',
    '    finish_class_type(typeinfo_types[(int)tik_user]);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_typeinfo_record(typeinfo_types[(int)tik_user], tik_user, 0);\n'
    '#endif\n')
object_source_overlay(eh_lower,
    '    finish_class_type(*type_ptr);\n',
    '    finish_class_type(*type_ptr);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_typeinfo_record(*type_ptr, kind,\n'
    '#if IA64_ABI\n      kind == tik_vmi_class ? array_num_bases : 0\n'
    '#else\n      0\n#endif\n    );\n#endif\n')
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

# Observe the exact generated storage chosen for an original temporary DIP.
object_source_overlay(init_lower,
    '      dip->variable = temp_var;\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_record_temporary_binding(dip, temp_var,\n'
    '        innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '      dip->variable = temp_var;\n')

# Retain source destruction identity through stock expression/ABI copies and
# observe the typed position before it is represented as region-table handles.
il_source = root / "upstream/src/il.c"
object_source_overlay(il_source,
    '  con->variant.template_param.variant.expr->compiler_generated = TRUE;\n'
    '  release_local_constant(&one_val);',
    '  con->variant.template_param.variant.expr->compiler_generated = TRUE;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_dependent_enum_increment(\n'
    '      con->variant.template_param.variant.expr, con->type, prev_val->type != con->type, type_of_unknown_templ_param_nontype);\n'
    '#endif\n'
    '  release_local_constant(&one_val);')
object_source_overlay(il_source,
    '#include "trans_copy.h"',
    '#include "trans_copy.h"\n#if defined(EDG_DOT_BACKEND)\n#include "dot_gen_object_source.h"\n#endif')
# Only signatures already owned by the exact lambda factory inherit its
# recipe when stock copy_type_full duplicates them for later declarations.
object_source_overlay(il_source,
    '}  /* copy_type_full */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_lambda_prototype_type_copy(from, to);\n'
    '#endif\n}  /* copy_type_full */')
object_source_overlay(il_source,
    '  return new_dip;\n}  /* i_copy_dynamic_init */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_dynamic_init_copy(dip, new_dip);\n'
    '#endif\n'
    '  return new_dip;\n}  /* i_copy_dynamic_init */')
if 'has_subobject_vtable ? subobject_vtable_handle.variable : NULL' not in eh_lower.read_text():
    object_source_overlay(eh_lower,
        '                                     &dedp->region_number,\n'
        '                                     insert_location);\n#if DO_UNORDERED_EH_PROCESSING',
        '                                     &dedp->region_number,\n'
        '                                     insert_location);\n'
        '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
        '  dot_object_source::observe_cleanup_region(dip, innermost_function_scope->variant.routine.ptr);\n'
        '#endif\n#if DO_UNORDERED_EH_PROCESSING')
if 'dot_object_source::observe_cleanup_region_copy(orig_region_table_entry' not in eh_lower.read_text():
    object_source_overlay(eh_lower,
        '  dedp->cleanup_state_to_set_when_starting_destruction = next_dip;\n'
        '#if DO_UNORDERED_EH_PROCESSING\n  if (dip->unordered) {\n'
        '    /* The destruction is unordered with respect to some surrounding\n',
        '  dedp->cleanup_state_to_set_when_starting_destruction = next_dip;\n'
        '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
        '  dot_object_source::observe_cleanup_region(dip, innermost_function_scope->variant.routine.ptr);\n'
        '#endif\n'
        '#if DO_UNORDERED_EH_PROCESSING\n  if (dip->unordered) {\n'
        '    /* The destruction is unordered with respect to some surrounding\n')

# Observe the exact VTT local/parameter at its typed lowering boundary. The
# original handle remains stock EDG metadata; no generated name/offset guessing.
object_source_overlay(eh_lower,
    '                                     insert_location);\n'
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '  dot_object_source::observe_cleanup_region(dip, innermost_function_scope->variant.routine.ptr);\n',
    '                                     insert_location);\n'
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '  dot_object_source::observe_cleanup_region(dip, innermost_function_scope->variant.routine.ptr,\n'
    '      has_subobject_vtable ? subobject_vtable_handle.variable : NULL);\n')
object_source_overlay(eh_lower,
    '  dedp->cleanup_state_to_set_when_starting_destruction = next_dip;\n'
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '  dot_object_source::observe_cleanup_region(dip, innermost_function_scope->variant.routine.ptr);\n',
    '  dedp->cleanup_state_to_set_when_starting_destruction = next_dip;\n'
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '  dot_object_source::observe_cleanup_region_copy(orig_region_table_entry, dip, innermost_function_scope->variant.routine.ptr);\n')

# The stock array runtime owns partial prefix construction and normal reverse
# destruction; retain the exact originating destruction and callback.
object_source_overlay(init_lower,
    '  (void)insert_expr_statement_set_pos(call_node, insert_location);\n'
    '}  /* add_array_constructor_call */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_array_runtime(dip, dtor_routine, call_node, innermost_function_scope->variant.routine.ptr, true);\n'
    '#endif\n'
    '  (void)insert_expr_statement_set_pos(call_node, insert_location);\n'
    '}  /* add_array_constructor_call */')
object_source_overlay(init_lower,
    '                                     /*free_storage=*/FALSE);\n'
    '    /* Make a statement containing the call and insert it at the right\n',
    '                                     /*free_storage=*/FALSE);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_array_runtime(NULL, dtor_routine, call_node, innermost_function_scope->variant.routine.ptr, false);\n'
    '#endif\n'
    '    /* Make a statement containing the call and insert it at the right\n')

# Preserve an exact copied lifetime boundary; the original and copy are both
# known here, before pop may discard a lifetime with no destruction obligations.
object_source_overlay(il_source,
    '          bind_object_lifetime(curr_object_lifetime, iek_expr_node,\n'
    '                               (char *)expr_copy);\n'
    '          (void)pop_object_lifetime();',
    '          bind_object_lifetime(curr_object_lifetime, iek_expr_node,\n'
    '                               (char *)expr_copy);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '          dot_object_source::observe_lifetime_copy(expr->variant.object_lifetime.ptr, curr_object_lifetime);\n'
    '#endif\n'
    '          (void)pop_object_lifetime();')

# Associate retained ordinary destructor calls with exact source obligations.
# The observation scope changes no frontend inputs, insertion, or inlining.
object_source_overlay(init_lower,
    '  check_assertion(dip->destructor != NULL);\n'
    '  /* Set the cleanup state to what it should be after the destruction,',
    '  check_assertion(dip->destructor != NULL);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::NormalDestructionCapture dot_object_normal_destruction(dip, innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n'
    '  /* Set the cleanup state to what it should be after the destruction,')
object_source_overlay(init_lower,
    '  add_destructor_call(dip->destructor, ipdp, have_complete_object,\n'
    '                      vtt_addr_node, insert_location);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::NormalDestructionCapture dot_object_normal_destruction(dip, innermost_function_scope->variant.routine.ptr);\n'
    '#endif\n'
    '  add_destructor_call(dip->destructor, ipdp, have_complete_object,\n'
    '                      vtt_addr_node, insert_location);')
# Upgrade the exact existing call observer without stacking its prior form.
old_entity_call_observer = ('#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_normal_destruction_call(routine, call_node, top_level_node,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n')
entity_call_source = init_lower.read_text()
if old_entity_call_observer in entity_call_source:
    init_lower.write_text(entity_call_source.replace(old_entity_call_observer, ''))
object_source_overlay(init_lower,
    '  return top_level_node;\n}  /* make_call_node_full */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_normal_destruction_call(routine, call_node, top_level_node,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '  dot_object_source::observe_generated_entity_initializer_call(routine, top_level_node,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '  return top_level_node;\n}  /* make_call_node_full */')

object_source_overlay(il_source,
    '  return expr_copy;\n}  /* i_copy_expr_tree */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_throw_copy(expr, expr_copy,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '  dot_object_source::observe_record_transfer_copy(expr, expr_copy,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '  dot_object_source::observe_virtual_base_adjustment_copy(expr, expr_copy,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '  dot_object_source::observe_normal_destruction_copy(expr, expr_copy,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '  return expr_copy;\n}  /* i_copy_expr_tree */')

# Observe only exact already-lowered initialization/temporary assignments.
# insert_expr may turn an observed assignment into a comma by shallow-copying
# its old root first. Preserve that exact correspondence before the old root
# changes kind; recursive i_copy_expr_tree refreshes copied operands separately.
object_source_overlay(il_source,
    '  return expr_copy;\n}  /* copy_node */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_throw_copy(expr, expr_copy,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '  dot_object_source::observe_record_transfer_copy(expr, expr_copy,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '  dot_object_source::observe_virtual_base_adjustment_copy(expr, expr_copy,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '  return expr_copy;\n}  /* copy_node */')
object_source_overlay(il_source,
    '}  /* overwrite_node */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_virtual_base_adjustment_overwrite(node, source_node,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '}  /* overwrite_node */')
# Runtime array helpers may replace the original new/delete expression root.
# Preserve that exact replacement and invalidate any overwritten old proof.
object_source_overlay(il_source,
    '}  /* overwrite_node */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_array_protocol_overwrite(node, source_node,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '}  /* overwrite_node */')
# Migrate the first virtual-base observer, which watched a final class lvalue.
# Related-class lowering cancels &* and may overwrite that node; only the
# pointer result before add_indirection owns the dynamic adjustment now.
obsolete_virtual_hooks = (
    '#if defined(EDG_DOT_BACKEND) && IA64_ABI\n'
    '  if (!complete_object) dot_object_source::observe_virtual_base_adjustment(\n'
    '    node, dot_original_virtual_base_object, second_use, vbase_offset, class_type, bcp,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_original_virtual_base_object = node;\n'
    '#endif\n'
    '    node = add_address_of_to_node(node);\n',
)
source = lower.read_text()
source = source.replace(obsolete_virtual_hooks[0], '')
source = source.replace(obsolete_virtual_hooks[1], '    node = add_address_of_to_node(node);\n')
lower.write_text(source)
object_source_overlay(lower,
    '  a_type_ptr       class_type, data_section_class_type;\n',
    '  a_type_ptr       class_type, data_section_class_type;\n'
    '#if defined(EDG_DOT_BACKEND) && IA64_ABI\n'
    '  an_expr_node_ptr dot_original_virtual_base_object = NULL;\n'
    '#endif\n')
object_source_overlay(lower,
    '    node = add_address_of_to_node(node);\n'
    '    /* The vbase offset is stored in bytes.',
    '    node = add_address_of_to_node(node);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_original_virtual_base_object = node;\n'
    '#endif\n'
    '    /* The vbase offset is stored in bytes.')
object_source_overlay(lower,
    '    /* Convert back to lvalue. */\n'
    '    node = add_indirection_to_node(node);\n',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_virtual_base_adjustment(\n'
    '      node, dot_original_virtual_base_object, second_use, vbase_offset, class_type, bcp,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '    /* Convert back to lvalue. */\n'
    '    node = add_indirection_to_node(node);\n')
source = lower.read_text()
virtual_function = source[source.index('static an_expr_node_ptr make_vbase_class_lvalue('):source.index('}  /* make_vbase_class_lvalue */')]
if virtual_function.count('dot_object_source::observe_virtual_base_adjustment(') != 1 or virtual_function.count('dot_original_virtual_base_object = node;') != 1:
    raise RuntimeError('Virtual-base adjustment observer must have exactly one pointer-result capture')
object_source_overlay(lower,
    '  release_local_constant(&null_constant);\n}  /* add_null_preservation_code */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_virtual_base_null_preservation(expr, source_node);\n'
    '#endif\n'
    '  release_local_constant(&null_constant);\n}  /* add_null_preservation_code */')
object_source_overlay(lower,
    '  *temp_var = assoc_var;\n'
    '  return troublesome;\n}  /* check_for_troublesome_aggregate_constant */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  if (troublesome) dot_object_source::observe_pooled_record_constant(constant, assoc_var);\n'
    '#endif\n'
    '  *temp_var = assoc_var;\n'
    '  return troublesome;\n}  /* check_for_troublesome_aggregate_constant */')
# Retire the unused conditional-specific experiment from previously prepared
# trees. The stock path promotes an already-owned initialization via overwrite.
source = lower.read_text()
source = source.replace(
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::ConditionalRecordConstantInitialization dot_conditional_constant(\n'
    '          new_expr, master_dip,\n'
    '          innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n', '')
source = source.replace(
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_conditional_constant.finish(expr);\n'
    '#endif\n', '')
lower.write_text(source)
object_source_overlay(il_source,
    '}  /* overwrite_node */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_record_transfer_overwrite(node, source_node,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '}  /* overwrite_node */')
# Upgrade the existing complete-object observer as one idempotent block.
# Remove the short-lived separate hook if a prepared tree already contains it.
source = init_lower.read_text()
source = source.replace(
    '#if defined(EDG_DOT_BACKEND)\n'
    '    if (have_complete_object) dot_object_source::observe_record_transfer(assign_node,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "construction");\n'
    '#endif\n', '')
source = source.replace(
    '#if defined(EDG_DOT_BACKEND)\n'
    '    if (have_complete_object) dot_object_source::observe_record_storage_copy(assign_node,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "initializer-storage");\n'
    '#endif\n', '')
source = source.replace(
    '    if (have_complete_object) dot_object_source::observe_record_constant_initialization(assign_node, dip, con);\n'
    '#endif\n',
    '    if (have_complete_object) dot_object_source::observe_record_constant_initialization(assign_node, dip, con);\n'
    '    if (have_complete_object) dot_object_source::observe_record_storage_copy(assign_node,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "initializer-storage");\n'
    '#endif\n')
init_lower.write_text(source)
object_source_overlay(init_lower,
    '    assign_stmt = insert_expr_statement(assign_node, insert_location);\n'
    '    set_stmt_pos_to_code_pos_for_lowering(assign_stmt);\n'
    '  }  /* if */\n}  /* add_init_assignment */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    if (have_complete_object) dot_object_source::observe_record_transfer(assign_node,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "construction");\n'
    '    if (have_complete_object) dot_object_source::observe_record_constant_initialization(assign_node, dip, con);\n'
    '    if (have_complete_object) dot_object_source::observe_record_storage_copy(assign_node,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "initializer-storage");\n'
    '#endif\n'
    '    assign_stmt = insert_expr_statement(assign_node, insert_location);\n'
    '    set_stmt_pos_to_code_pos_for_lowering(assign_stmt);\n'
    '  }  /* if */\n}  /* add_init_assignment */')
object_source_overlay(lower,
    '  return temp;\n}  /* assign_expr_to_temp */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_record_transfer(expr,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "temporary");\n'
    '#endif\n'
    '  return temp;\n}  /* assign_expr_to_temp */')
object_source_overlay(lower,
    '      temp_node = make_var_assignment_expr(temp, copy_node(expr));\n'
    '      (void)insert_expr_statement(temp_node, insert_location);',
    '      temp_node = make_var_assignment_expr(temp, copy_node(expr));\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_object_source::observe_record_transfer(temp_node,\n'
    '        innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "temporary");\n'
    '#endif\n'
    '      (void)insert_expr_statement(temp_node, insert_location);')


# These stock operations move already-evaluated class storage without invoking
# another C++ copy constructor or creating another destruction obligation.
object_source_overlay(init_lower,
    '  assign = make_var_assignment_expr(temp, rhs_expr);\n'
    '  /* Replace the original expression with a reference to the temporary. */',
    '  assign = make_var_assignment_expr(temp, rhs_expr);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_record_storage_copy(assign,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "evaluation-save");\n'
    '#endif\n'
    '  /* Replace the original expression with a reference to the temporary. */')
object_source_overlay(init_lower,
    '    if (temp_assignment != NULL) {\n'
    '      overwrite_node(expr, make_comma_node(temp_assignment, copy_node(expr)));\n'
    '    }  /* if */\n'
    '  }  /* if */\n'
    '  check_assertion(expr->is_lvalue == returns_lvalue);\n'
    '}  /* rewrite_class_assignment_if_necessary */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_record_storage_copy(expr,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "class-assignment");\n'
    '#endif\n'
    '    if (temp_assignment != NULL) {\n'
    '      overwrite_node(expr, make_comma_node(temp_assignment, copy_node(expr)));\n'
    '    }  /* if */\n'
    '  }  /* if */\n'
    '  check_assertion(expr->is_lvalue == returns_lvalue);\n'
    '}  /* rewrite_class_assignment_if_necessary */')


# Update the observer signature in an already prepared source overlay.
source = init_lower.read_text()
source = source.replace(
    'observe_array_deletion_virtual_callback(dtor_routine, dtor_addr_node);',
    'observe_array_deletion_virtual_callback(dtor_routine, dtor_addr_node, integer_type((an_integer_kind)targ_size_t_int_kind));')
source = source.replace(
    'observe_array_deletion_virtual_callback(dtor_routine, dtor_addr_node, size_t_type);',
    'observe_array_deletion_virtual_callback(dtor_routine, dtor_addr_node, integer_type((an_integer_kind)targ_size_t_int_kind));')
init_lower.write_text(source)
# Observe the precise GNU/IA64 virtual array destructor selection after stock
# lowering adds its zero-element cookie guard. No IL is changed.
object_source_overlay(init_lower,
    '      dtor_addr_node = make_operator_node((an_expr_operator_kind)eok_question,\n'
    '                                          dtor_addr_node->type, num_elem_node);',
    '      dtor_addr_node = make_operator_node((an_expr_operator_kind)eok_question,\n'
    '                                          dtor_addr_node->type, num_elem_node);\n'
    '#if defined(EDG_DOT_BACKEND) && IA64_ABI\n'
    '      dot_object_source::observe_array_deletion_virtual_callback(dtor_routine, dtor_addr_node, integer_type((an_integer_kind)targ_size_t_int_kind));\n'
    '#endif')

# Stock dynamic initialization and dik_bitwise_copy emit complete-object
# storage copies after resolving the C++ construction operation separately.
# Keep those exact factory origins distinct from an ordinary C++ assignment.
object_source_overlay(init_lower,
    '                                                          source_node);\n'
    '    (void)insert_expr_statement(assign_node, insert_location);',
    '                                                          source_node);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    if (have_complete_object) dot_object_source::observe_record_storage_copy(assign_node,\n'
    '      innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL, "bitwise-initialization");\n'
    '#endif\n'
    '    (void)insert_expr_statement(assign_node, insert_location);')

# This special variable-length zero initializer bypasses rollback setup;
# observe the exact stock branch instead of inventing a cleanup action.
object_source_overlay(init_lower,
    '        insert_runtime_zeroing_call(var_rvalue_expr(temp_var),\n'
    '                                    var_rvalue_expr(num_bytes_temp),\n'
    '                                    &insert_location);',
    '        insert_runtime_zeroing_call(var_rvalue_expr(temp_var),\n'
    '                                    var_rvalue_expr(num_bytes_temp),\n'
    '                                    &insert_location);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_object_source::observe_dynamic_array_zero_initialization(ndsp, dip,\n'
    '          innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif')

# Observe the completed per-thread first-byte publication at its exact stock factory.
object_source_overlay(init_lower,
    "static void set_local_static_guard_var(\n                                 a_variable_ptr         local_static_guard_var,\n                                 an_insert_location_ptr insert_location)\n/*\nlocal_static_guard_var is the guard variable associated with the initialization\nof a local static variable.  If necessary, add code to set the guard variable\nto indicate that the initialization is complete.  Insert the code at\n*insert_location.\n*/\n{\n  if (targ_ia64_abi_use_guard_acquire_release &&\n      !is_effective_thread_local(local_static_guard_var)) {\n    /* In non-thread-local cases, the call to __cxa_guard_release has already\n       been emitted, so there's nothing to do here. */\n  } else {\n    if (targ_ia64_abi_use_int_static_init_guard) {\n      /* ARM EABI specifies to use least significant bit for guard test. */\n      (void)insert_assignment_statement(var_lvalue_expr(\n                                                       local_static_guard_var),\n                                        (an_expr_operator_kind)eok_assign,\n                                        node_for_integer_constant(1L,\n                                                      (an_integer_kind)ik_int),\n                                        insert_location);\n    } else {\n      /* IA-64 ABI specifies to use first byte for guard test. */\n      (void)insert_assignment_statement(add_indirection_to_node(\n                                         add_cast_to_char_star(\n                                         var_addr_expr(\n                                                     local_static_guard_var))),\n                                        (an_expr_operator_kind)eok_assign,\n                                        node_for_integer_constant(1L,\n                                                     (an_integer_kind)ik_char),\n                                        insert_location);\n    }  /* if */\n  }  /* if */\n}  /* set_local_static_guard_var */",
    "static void set_local_static_guard_var(\n                                 a_variable_ptr         local_static_guard_var,\n                                 an_insert_location_ptr insert_location)\n/*\nlocal_static_guard_var is the guard variable associated with the initialization\nof a local static variable.  If necessary, add code to set the guard variable\nto indicate that the initialization is complete.  Insert the code at\n*insert_location.\n*/\n{\n  if (targ_ia64_abi_use_guard_acquire_release &&\n      !is_effective_thread_local(local_static_guard_var)) {\n    /* In non-thread-local cases, the call to __cxa_guard_release has already\n       been emitted, so there's nothing to do here. */\n  } else {\n    if (targ_ia64_abi_use_int_static_init_guard) {\n      /* ARM EABI specifies to use least significant bit for guard test. */\n      a_statement_ptr dot_guard_publication = insert_assignment_statement(var_lvalue_expr(\n                                                       local_static_guard_var),\n                                        (an_expr_operator_kind)eok_assign,\n                                        node_for_integer_constant(1L,\n                                                      (an_integer_kind)ik_int),\n                                        insert_location);\n#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n      dot_object_source::observe_local_thread_guard_publication(local_static_guard_var, dot_guard_publication);\n#endif\n    } else {\n      /* IA-64 ABI specifies to use first byte for guard test. */\n      a_statement_ptr dot_guard_publication = insert_assignment_statement(add_indirection_to_node(\n                                         add_cast_to_char_star(\n                                         var_addr_expr(\n                                                     local_static_guard_var))),\n                                        (an_expr_operator_kind)eok_assign,\n                                        node_for_integer_constant(1L,\n                                                     (an_integer_kind)ik_char),\n                                        insert_location);\n#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n      dot_object_source::observe_local_thread_guard_publication(local_static_guard_var, dot_guard_publication);\n#endif\n    }  /* if */\n  }  /* if */\n}  /* set_local_static_guard_var */")

# Own only the one-element zero/constructor helper created inside vector new.
object_source_overlay(init_lower,
    '    model_param_type = alloc_param_type(make_pointer_type(model_type));\n'
    '    (*last_param_type)->next = model_param_type;\n'
    '  }  /* if */\n'
    '  /* Build the definition of the routine.  */\n'
    '  scope = make_routine_definition(rp, /*make_return=*/TRUE,',
    '    model_param_type = alloc_param_type(make_pointer_type(model_type));\n'
    '    (*last_param_type)->next = model_param_type;\n'
    '  }  /* if */\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_generated_array_initializer_begin(rp, type, have_complete_object,\n'
    '    need_array_count, zero_entity, ctor_routine);\n'
    '#endif\n'
    '  /* Build the definition of the routine.  */\n'
    '  scope = make_routine_definition(rp, /*make_return=*/TRUE,')
object_source_overlay(init_lower,
    '                                            source_expr);\n'
    '  if (ctor_routine != NULL) {',
    '                                            source_expr);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_generated_array_initializer_copy(rp, scope, entity_var, model_var, copy_expr);\n'
    '#endif\n'
    '  if (ctor_routine != NULL) {')
object_source_overlay(init_lower,
    '    ctor_call = make_call_node(ctor_routine, ctor_entity_expr);\n'
    '    copy_expr = make_comma_node(copy_expr, ctor_call);',
    '    ctor_call = make_call_node(ctor_routine, ctor_entity_expr);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_generated_array_initializer_constructor(rp, ctor_call);\n'
    '#endif\n'
    '    copy_expr = make_comma_node(copy_expr, ctor_call);')
object_source_overlay(init_lower,
    '  /* Clean up. */\n'
    '  pop_generated_routine_context(scope, il_region, &context);\n'
    '  return rp;\n'
    '}  /* helper_routine_to_initialize_entity */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_generated_array_initializer_complete(rp, scope);\n'
    '#endif\n'
    '  /* Clean up. */\n'
    '  pop_generated_routine_context(scope, il_region, &context);\n'
    '  return rp;\n'
    '}  /* helper_routine_to_initialize_entity */')

# Typed throw initialization and exact runtime-address/start ownership.
object_source_overlay(init_lower,
    '  /* Make an "if" statement and insert it into the program. */\n'
    '  insert_if_statement(compare_node, /*is_initialization_guard=*/TRUE,',
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '  dot_object_source::observe_local_static_guard_probe(guarded_var, *test_var, compare_node,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '  /* Make an "if" statement and insert it into the program. */\n'
    '  insert_if_statement(compare_node, /*is_initialization_guard=*/TRUE,')
object_source_overlay(init_lower,
    '      /* Make the acquire call a boolean controlling expression. */',
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '      dot_object_source::observe_local_static_guard_calls(*test_var, acquire_node, release_node);\n'
    '#endif\n'
    '      /* Make the acquire call a boolean controlling expression. */')
object_source_overlay(init_lower,
    '    dip->is_guard_var_for_local_static_var_init = TRUE;',
    '    dip->is_guard_var_for_local_static_var_init = TRUE;\n'
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '    dot_object_source::observe_local_static_guard_cleanup(dip, local_static_guard_var,\n'
    '      innermost_function_scope->variant.routine.ptr);\n'
    '#endif')
for allocation_try_anchor in (
    '          try_expr = make_internal_try_expr(try_expr, delete_call);',
    '            try_expr = make_internal_try_expr(init_expr, delete_call);',
):
    object_source_overlay(init_lower, allocation_try_anchor,
        allocation_try_anchor + '\n'
        '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
        '  dot_object_source::observe_allocation_internal_try(try_expr, dyn_init_to_free_storage,\n'
        '    innermost_function_scope->variant.routine.ptr);\n'
        '#endif')
object_source_overlay(init_lower,
    '                                                 delete_routine, delete_args,\n'
    '                                                 zero_storage);',
    '                                                 delete_routine, delete_args,\n'
    '                                                 zero_storage);\n'
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '    dot_object_source::observe_allocation_internal_try(vec_new_node,\n'
    '      ndsp->freeing_of_storage_on_exception, innermost_function_scope->variant.routine.ptr);\n'
    '#endif')
object_source_overlay(eh_lower,
    '  /* In the non-portable schemes, we keep the original "try" statement. */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_try(tsp, innermost_function_scope->variant.routine.ptr, is_function_try_block);\n'
    '#endif\n'
    '  /* In the non-portable schemes, we keep the original "try" statement. */')
object_source_overlay(eh_lower,
    '    /* Lower the dependent statement of the catch clause. */',
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '    dot_object_source::HandlerCapture dot_object_handler_capture(handler);\n'
    '#endif\n'
    '    /* Lower the dependent statement of the catch clause. */')
object_source_overlay(init_lower,
    '  a_type_ptr         temp_type = NULL;\n\n  saved_code_pos = code_pos_for_lowering;',
    '  a_type_ptr         temp_type = NULL;\n'
    '#if defined(EDG_DOT_BACKEND) && !DO_FULL_PORTABLE_EH_LOWERING\n'
    '  dot_object_source::observe_payload_subobject(dip, ipdp,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n\n  saved_code_pos = code_pos_for_lowering;')
object_source_overlay(eh_lower,
    '  /* Check for a throw with no operand, i.e., a rethrow. */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::ThrowCapture dot_object_throw_capture(expr,\n'
    '    innermost_function_scope ? innermost_function_scope->variant.routine.ptr : NULL);\n'
    '#endif\n'
    '  /* Check for a throw with no operand, i.e., a rethrow. */')
object_source_overlay(eh_lower,
    '  return node;\n}  /* make_thrown_object_address_node */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_throw_construct(node, false);\n'
    '#endif\n'
    '  return node;\n}  /* make_thrown_object_address_node */')
object_source_overlay(eh_lower,
    '                          (a_lowered_eh_construct_kind)leck_exception_started);\n'
    '    (void)insert_expr_statement(node, insert_location);',
    '                          (a_lowered_eh_construct_kind)leck_exception_started);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_throw_construct(node, true);\n'
    '#endif\n'
    '    (void)insert_expr_statement(node, insert_location);')

# Observe stock TLS wrapper/alias factories, preserving their original variables.
object_source_overlay(init_lower,
    '  return var->init_routine.thread.init_routine;\n}  /* thread_local_init_routine_for_variable */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_tls_variable_initializer(var);\n'
    '#endif\n'
    '  return var->init_routine.thread.init_routine;\n}  /* thread_local_init_routine_for_variable */')
object_source_overlay(init_lower,
    '    pop_generated_routine_context(scope, region_number, &grcontext);\n'
    '  }  /* if */\n'
    '  return var->init_routine.thread.wrapper;',
    '    pop_generated_routine_context(scope, region_number, &grcontext);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_tls_variable_wrapper(var, scope, call_insert_location != &insert_location);\n'
    '#endif\n'
    '  }  /* if */\n'
    '  return var->init_routine.thread.wrapper;')
object_source_overlay(init_lower,
    '}  /* make_null_tls_init_routine */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_object_source::observe_empty_tls_initializer(routine, scope);\n'
    '#endif\n'
    '}  /* make_null_tls_init_routine */')

# Reserve the exact stock wrapper before traversal can mistake it for a source declaration.
object_source_overlay(init_lower,
    '    var->init_routine.thread.wrapper = wrapper_routine;\n',
    '    var->init_routine.thread.wrapper = wrapper_routine;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_object_source::observe_tls_variable_wrapper_created(var);\n'
    '#endif\n')

# TLS declarations referenced from another TU may be absent from scope.variables.
# Observe their original declaration before lower_variable mutates type/storage.
object_source_overlay(lower,
    '  if (!visited_yet(variable)) {\n    check_assertion(!ignore_variable_in_back_end(variable));',
    '  if (!visited_yet(variable)) {\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    if (dot_object_source::enabled() && is_effective_thread_local(variable))\n'
    '      dot_object_source::capture_variable(variable);\n'
    '#endif\n'
    '    check_assertion(!ignore_variable_in_back_end(variable));')

# Relational semantic backend: direct program rows, no legacy raw graph capture.
object_source_overlay(init_lower,
    '    pop_generated_routine_context(new_routine_scope, new_routine_il_region,\n'
    '                                  &grcontext);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::lambda_entry_complete(new_routine, new_routine_scope);\n'
    '    dot_semantic::abi_entry_complete(routine, new_routine, new_routine_scope);\n'
    '#endif\n'
    '    pop_generated_routine_context(new_routine_scope, new_routine_il_region,\n'
    '                                  &grcontext);')
semantic_schema = (root / "object-linker/relational/schema.sql").read_text()
semantic_schema_header = root / "upstream/src/dot_gen_semantic_schema.h"
semantic_schema_text = ('#pragma once\nstatic const char dot_semantic_schema[] = R"DOTSCHEMA(' +
                        semantic_schema + ')DOTSCHEMA";\n')
if not semantic_schema_header.exists() or semantic_schema_header.read_text() != semantic_schema_text:
    semantic_schema_header.write_text(semantic_schema_text)
for filename in ("lower_il.c", "lower_c99.c"):
    object_source_overlay(root / "upstream/src" / filename,
        '#include "dot_gen_object_source.h"',
        '#include "dot_gen_object_source.h"\n#include "dot_gen_semantic.h"')
    object_source_overlay(root / "upstream/src" / filename,
        '  dot_object_source::observe_scope(scope);',
        '  dot_semantic::capture_scope(scope);\n  dot_object_source::observe_scope(scope);')

# Bind only requested static typeid facts to the persistent RTTI variable
# returned by stock selection. The callback does not generate RTTI storage.
object_source_overlay(eh_lower,
    '#include "dot_gen_object_source.h"',
    '#include "dot_gen_object_source.h"\n#include "dot_gen_semantic.h"')
for selection in ("make_typeinfo_var", "get_typeinfo_var"):
    object_source_overlay(eh_lower,
        '  return typeinfo_var;\n}  /* ' + selection + ' */',
        '#if defined(EDG_DOT_BACKEND)\n'
        '  dot_semantic::typeinfo_storage_selected(type, typeinfo_var);\n'
        '#endif\n'
        '  return typeinfo_var;\n}  /* ' + selection + ' */')

# Preserve the linker identity selected by stock lowering for a string object.
# The callback observes the promoted variable; it does not create or pool literals.
object_source_overlay(lower,
    '  return string_var;\n}  /* get_variable_for_string_constant */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::literal_storage_selected(constant, string_var);\n'
    '#endif\n'
    '  return string_var;\n}  /* get_variable_for_string_constant */')
object_source_overlay(lower,
    '  lower_top_level_scope(scope, memory_region);\n}  /* lower_function_scope */',
    '  lower_top_level_scope(scope, memory_region);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::literal_function_complete(routine);\n'
    '#endif\n'
    '}  /* lower_function_scope */')

# Targeted relational dispatch facts at stock EDG decision points. These hooks
# observe selected entries and writes; they neither change IL nor select overrides.
object_source_overlay(root / "upstream/src/lower_init.c",
    '#include "dot_gen_object_source.h"',
    '#include "dot_gen_object_source.h"\n#include "dot_gen_semantic.h"')
# Preserve only the selected size/check graph for a source allocation expression.
object_source_overlay(init_lower,
    '  number_of_elements = ndsp->number_of_elements;',
    '  number_of_elements = ndsp->number_of_elements;\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::allocation_size_begin(ndsp);\n'
    '#endif')
object_source_overlay(init_lower,
    '  temp = make_lowered_temporary(num_elements_type);',
    '  temp = make_lowered_temporary(num_elements_type);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::allocation_count_binding(temp);\n'
    '#endif')
object_source_overlay(init_lower,
    '      temp = make_lowered_temporary(number_of_elements->type);',
    '      temp = make_lowered_temporary(number_of_elements->type);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_semantic::allocation_effective_count_binding(temp, number_of_elements);\n'
    '#endif')
object_source_overlay(init_lower,
    '  insert_expr(call_node, &then_insert_location);\n'
    '  release_local_constant(&zero_constant);\n'
    '  release_local_constant(&elem_size_constant);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::allocation_length_check(test_node, throw_bad_array_new_length_routine);\n'
    '#endif\n'
    '  insert_expr(call_node, &then_insert_location);\n'
    '  release_local_constant(&zero_constant);\n'
    '  release_local_constant(&elem_size_constant);')
object_source_overlay(init_lower,
    '  return number_of_bytes;\n}  /* size_arg_for_new */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::allocation_size_end(ndsp, number_of_bytes);\n'
    '#endif\n'
    '  return number_of_bytes;\n}  /* size_arg_for_new */')
# Observe the exact stock decision for initialization after allocation. The
# correspondence already exists for requested source allocation expressions.
object_source_overlay(init_lower,
    '  return result;\n}  /* null_check_needed */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::allocation_construction_null_check(ndsp, result);\n'
    '#endif\n'
    '  return result;\n}  /* null_check_needed */')
# Observe the scalar runtime-array byte-zero action at its actual stock call.
# num_bytes_temp already holds the exact captured allocation size.
object_source_overlay(init_lower,
    '        insert_runtime_zeroing_call(var_rvalue_expr(temp_var),\n'
    '                                    var_rvalue_expr(num_bytes_temp),',
    '#if defined(EDG_DOT_BACKEND)\n'
    '        dot_semantic::allocation_zeroing(ndsp, num_bytes_temp);\n'
    '#endif\n'
    '        insert_runtime_zeroing_call(var_rvalue_expr(temp_var),\n'
    '                                    var_rvalue_expr(num_bytes_temp),')
object_source_overlay(init_lower,
    '    change_to_cast(expr, call_node, expr->type);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::allocation_simple_result(ndsp);\n'
    '#endif\n'
    '    change_to_cast(expr, call_node, expr->type);')
object_source_overlay(init_lower,
    '    call_node = make_delete_call_node(delete_routine, ndsp->type, ptr_node);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::deallocation_begin(ndsp);\n'
    '#endif\n'
    '    call_node = make_delete_call_node(delete_routine, ndsp->type, ptr_node);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::deallocation_end(ndsp);\n'
    '#endif')
object_source_overlay(init_lower,
    '  arg_node = modify_delete_call_args(delete_routine, delete_type, arg_node);\n'
    '  call_node = make_call_node(delete_routine, arg_node);',
    '  arg_node = modify_delete_call_args(delete_routine, delete_type, arg_node);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::deallocation_arguments(delete_routine, arg_node);\n'
    '#endif\n'
    '  call_node = make_call_node(delete_routine, arg_node);')
object_source_overlay(init_lower,
    '    dtor_call_node = make_dtor_call_for_delete(dip, ptr_node, delete_routine);',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::deallocation_begin(ndsp);\n'
    '#endif\n'
    '    dtor_call_node = make_dtor_call_for_delete(dip, ptr_node, delete_routine);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::deallocation_end(ndsp);\n'
    '#endif')
# Selected complete/deleting destructor, null guard, and reusable pointer identities.
object_source_overlay(init_lower,
    '  /* Make a call of the destructor. */\n'
    '  call_node = make_call_node_full(dtor_routine, ptr_node,',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::deallocation_destruction(dip, dtor_routine, ptr_node,\n'
    '    ptr_node_delete, need_null_ptr_test);\n'
    '#endif\n'
    '  /* Make a call of the destructor. */\n'
    '  call_node = make_call_node_full(dtor_routine, ptr_node,')
object_source_overlay(init_lower,
    '  check_assertion(call_node != NULL);\n'
    '  if (dtor_routine->is_virtual) {',
    '  check_assertion(call_node != NULL);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::deallocation_virtual_call(dip, dtor_routine, call_node);\n'
    '#endif\n'
    '  if (dtor_routine->is_virtual) {')
object_source_overlay(lower,
    '}  /* promote_static_variable_out_of_function */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::static_local_storage_selected(variable, routine);\n'
    '#endif\n'
    '}  /* promote_static_variable_out_of_function */')
object_source_overlay(init_lower,
    '}  /* copy_non_static_data_member_initializers_if_necessary */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::default_member_initializers_complete(scope);\n'
    '#endif\n'
    '}  /* copy_non_static_data_member_initializers_if_necessary */')
object_source_overlay(lower,
    '    dot_object_source::observe_record_before_prelowering(class_type);',
    '    dot_semantic::virtual_source_class(class_type);\n'
    '    dot_object_source::observe_record_before_prelowering(class_type);')
object_source_overlay(lower,
    '  dot_object_source::observe_routine(routine);',
    '  dot_semantic::virtual_source_routine(routine);\n'
    '  dot_object_source::observe_routine(routine);')
object_source_overlay(lower,
    '    /* Put out the body of the table. */',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::virtual_table_begin(class_type, bcp, ctor_bcp, vtbl_var,\n'
    '#if IA64_ABI\n'
    '      (num_negative_vtable_entries(class_type, bcp) +\n'
    '       (bcp != NULL && ctor_bcp == NULL ? bcp->virtual_function_table_offset : 0)) * vtbl_entry_size(),\n'
    '#else\n'
    '      0,\n'
    '#endif\n'
    '#if IA64_ABI\n'
    '      vtbl_entry_size()\n'
    '#else\n'
    '      make_vtbl_entry_type()->size\n'
    '#endif\n'
    '    );\n'
    '#endif\n'
    '    /* Put out the body of the table. */')
object_source_overlay(lower,
    '                                &next_entry_number, first_virtual);',
    '                                &next_entry_number, first_virtual);\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::virtual_table_end();\n'
    '#endif')
object_source_overlay(lower,
    '    add_vtbl_entry_init(delta,\n#if IA64_ABI\n                        use_null_function ?',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::virtual_table_entry(entry_number, primary_function, func_to_call, delta,\n'
    '#if IA64_ABI\n'
    '      use_null_function\n'
    '#else\n'
    '      FALSE\n'
    '#endif\n'
    '    );\n'
    '#endif\n'
    '    add_vtbl_entry_init(delta,\n#if IA64_ABI\n                        use_null_function ?')
object_source_overlay(lower,
    '      add_vtbl_entry_init(delta,\n#if IA64_ABI\n                          use_null_function ?',
    '#if defined(EDG_DOT_BACKEND)\n'
    '      dot_semantic::virtual_table_entry(entry_number + 1, primary_function, second_func_to_call, delta, use_null_function);\n'
    '#endif\n'
    '      add_vtbl_entry_init(delta,\n#if IA64_ABI\n                          use_null_function ?')
ctor_vptr_call = ('  initialize_vptrs_in_class(class_type, this_param_var, construction_vtbls_var,\n'
                 '                            (a_constant_ptr)NULL, insert_location);')
object_source_overlay(init_lower, ctor_vptr_call,
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::virtual_vptr_begin(innermost_function_scope->variant.routine.ptr, "after-bases-before-members");\n'
    '#endif\n' + ctor_vptr_call + '\n'
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::virtual_vptr_end();\n'
    '#endif')
object_source_overlay(init_lower,
    '  insert_primary_vtbl_assignment(class_type, this_param_var,\n'
    '                                 dtor_info.destruction_vtbls_var,',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::virtual_vptr_begin(innermost_function_scope->variant.routine.ptr, "before-body");\n'
    '#endif\n'
    '  insert_primary_vtbl_assignment(class_type, this_param_var,\n'
    '                                 dtor_info.destruction_vtbls_var,')
object_source_overlay(init_lower,
    '  /* Now generate epilogue wrapper code to destroy members and base classes.',
    '#if defined(EDG_DOT_BACKEND)\n'
    '  dot_semantic::virtual_vptr_end();\n'
    '#endif\n'
    '  /* Now generate epilogue wrapper code to destroy members and base classes.')
object_source_overlay(init_lower,
    '    initialize_vptr(vtbl_addr_node,\n'
    '                    aggr_con == NULL ?',
    '#if defined(EDG_DOT_BACKEND)\n'
    '    dot_semantic::virtual_vptr_write(class_type, (a_base_class_ptr)NULL, vtbl_addr_node,\n'
    '#if IA64_ABI\n'
    '      ctor_vtbl_var != NULL,\n'
    '#else\n'
    '      FALSE,\n'
    '#endif\n'
    '      0);\n'
    '#endif\n'
    '    initialize_vptr(vtbl_addr_node,\n'
    '                    aggr_con == NULL ?')
base_vptr_hook = ('#if defined(EDG_DOT_BACKEND)\n'
    '      dot_semantic::virtual_vptr_write(class_type, bcp, vtbl_addr_node,\n'
    '#if ABI_CHANGES_FOR_CONSTRUCTION_VTBLS\n'
    '        bcp->index_in_construction_vtbl_array != 0, bcp->index_in_construction_vtbl_array\n'
    '#else\n'
    '        FALSE, 0\n'
    '#endif\n'
    '      );\n'
    '#endif\n')
object_source_overlay(init_lower,
    '      initialize_vptr(vtbl_addr_node, vptr_node, bcp_con, insert_location);',
    base_vptr_hook +
    '      initialize_vptr(vtbl_addr_node, vptr_node, bcp_con, insert_location);')
object_source_overlay(init_lower,
    '      vptr_node = make_base_class_lvalue_from_var(this_param_var, bcp,\n'
    '                                                  /*complete_object=*/FALSE);',
    base_vptr_hook +
    '      vptr_node = make_base_class_lvalue_from_var(this_param_var, bcp,\n'
    '                                                  /*complete_object=*/FALSE);')
