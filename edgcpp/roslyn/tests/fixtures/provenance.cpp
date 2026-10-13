#include "provenance_nested.hpp"
#include <angled.hpp>
#include "system_entry.hpp"
#define PROVENANCE_NAME first_included_value
#include "provenance_repeat.hpp"
#undef PROVENANCE_NAME
#define PROVENANCE_NAME second_included_value
#include "provenance_repeat.hpp"
#undef PROVENANCE_NAME
#line 900 "presumed_translation.cpp"
int remapped_value = __LINE__;
int command_line_value = PROVENANCE_CMD_LINE;
int macros_only_value = PROVENANCE_MACROS_ONLY;
int predefined_value = __STDC_HOSTED__;
int provenance_sum() {
  return nested_physical_line + nested_presumed_line + first_included_value +
         second_included_value + remapped_value + command_line_value +
         macros_only_value + predefined_value + preincluded_value +
         angled_user_value + quoted_system_value + system_entry_value;
}
