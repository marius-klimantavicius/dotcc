#!/usr/bin/env python3
"""Install the version-controlled backend overlay into the pinned EDG checkout."""
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[1]
for source, target in [
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
