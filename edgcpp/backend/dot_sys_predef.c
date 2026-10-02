/* Custom translated-program target hook; upstream EDG semantics are unchanged.
 * Keep EDG's builtins and dialect setup, replacing only its system macro hook.
 */
#define enter_system_specific_predefined_macros_and_assertions \
        edg_host_system_predefined_macros_and_assertions
#include "sys_predef.c"
#undef enter_system_specific_predefined_macros_and_assertions

BEGIN_EDG_NAMESPACE

void enter_system_specific_predefined_macros_and_assertions(void) {
  const char *profile = getenv("EDG_DOT_TARGET");
  if (profile == NULL || strcmp(profile, "legacy-linux-x64") == 0) {
    edg_host_system_predefined_macros_and_assertions();
    return;
  }
  if (strcmp(profile, "portable-lp64-binary80-v1") != 0) {
    fprintf(stderr, "dot_gen_be: unknown target profile: %s\n", profile);
    exit(1);
  }
  // Reject mismatched frontend layout/constant evaluation before defining the
  // portable header contract. The profile is independent of the runtime OS.
  if (!targ_little_endian || targ_char_bit != 8 || !targ_has_signed_chars ||
      targ_sizeof_short != 2 || targ_alignof_short != 2 ||
      targ_sizeof_pointer != 8 || targ_alignof_pointer != 8 ||
      targ_sizeof_int != 4 || targ_alignof_int != 4 ||
      targ_sizeof_long != 8 || targ_alignof_long != 8 ||
      targ_sizeof_long_long != 8 || targ_alignof_long_long != 8 ||
      targ_sizeof_int128 != 16 || targ_alignof_int128 != 16 ||
      targ_sizeof_float != 4 || targ_alignof_float != 4 ||
      targ_sizeof_double != 8 || targ_alignof_double != 8 ||
      targ_flt_mant_dig != 24 || targ_flt_min_exp != -125 || targ_flt_max_exp != 128 ||
      targ_dbl_mant_dig != 53 || targ_dbl_min_exp != -1021 || targ_dbl_max_exp != 1024 ||
      targ_sizeof_long_double != 16 ||
      targ_alignof_long_double != 16 || targ_ldbl_mant_dig != 64 ||
      targ_ldbl_min_exp != -16381 || targ_ldbl_max_exp != 16384 ||
      targ_wchar_t_int_kind != ik_int || targ_wint_t_int_kind != ik_unsigned_int ||
      targ_sizeof_wchar_t != 4 || targ_sizeof_char16_t != 2 || targ_sizeof_char32_t != 4 ||
      targ_size_t_int_kind != ik_unsigned_long ||
      targ_ptrdiff_t_int_kind != ik_long) {
    fprintf(stderr, "dot_gen_be: frontend layout does not match portable-lp64-binary80-v1\n");
    exit(1);
  }
  // macro.c reads the host file after this hook. Never allow it to reintroduce
  // OS/CPU macros, including when cpfe-dot is invoked outside dotnet.py.
  use_predefined_macro_file = FALSE;
#define DOT_TARGET_MACRO(name, value) \
  (void)enter_predef_macro(value, #name, TRUE, FALSE);
#include "dot_gen_target_macros.def"
#undef DOT_TARGET_MACRO
}

END_EDG_NAMESPACE
