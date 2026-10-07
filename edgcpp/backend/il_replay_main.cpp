// Optional lowered-IL reader. No parsing or lowering takes place here.
// Initialization follows standalone_utility_{early,late}_init in fe_init.c.
// Full frontend support is linked because dot_gen_be uses EDG's type helpers.
#include "fe_common.h"
#include "il.h"
#include "il_read.h"
#include "il_file.h"
#include "mem_manage.h"
#include "target.h"
#include "error.h"
#include "il_to_str.h"
#include "float_pt.h"
#include "floating.h"
#include "lower_eh.h"

void back_end(void);
USING_NAMESPACE_EDG

int main(int argc, char **argv) {
  if (argc != 2) {
    fprintf(stderr, "usage: dot-il-reader checkpoint.cil\n");
    return 2;
  }
  mem_manage_early_init();
  host_envir_early_init();
  target_early_init();
  error_early_init();
  primary_source_file_name = nullptr;
  FILE *input = fopen(argv[1], "rb");
  if (!input) { perror(argv[1]); return 2; }
  il_read(input);
  // init_flags_and_types restores the stock C backend's query context. Dot's
  // promotion view additionally observes these recorded emulation settings.
  target_configuration_index = il_header.target_configuration_index;
  set_target_configuration(il_header.target_configuration_index);
  clang_mode = il_header.clang_mode;
  clang_version = il_header.clang_version;
  // cmd_line.c defaults this to true and disables it for MSVC, PCC and GCC<4.
  // The public dotnet driver has no custom override for this frontend flag.
  bit_field_promotion_applies_to_some_operations =
      !microsoft_mode && C_dialect != C_dialect_pcc &&
      (!gnu_mode || gnu_version >= 40000);
  target_one_time_init();
  il_to_str_one_time_init();
  error_one_time_init();
  target_init();
  il_to_str_init();
  error_init();
  float_pt_init();
#if !USE_HOST_FP_CONVERSION_ROUTINES || \
    (USE_FLOAT128_FOR_HOST_FP_VALUE && !USE_QUADMATH_LIBRARY)
  floating_one_time_init();
#endif
  // null_eh_region_number is target-sized process state, not serialized IL.
  eh_lower_init();
  primary_source_file_name = il_header.primary_source_file->file_name;
  for (a_memory_region_number region = 1;
       region <= highest_used_region_number; ++region) {
    if (index_for_il_file[region] && !il_header.region_scope_entry[region])
      read_memory_region(region);
  }
  back_end();
  fclose(input);
  return 0;
}
