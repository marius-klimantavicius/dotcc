//type:fp
//options_all:--clang_v 170000

constexpr bool is_equal(const char *LHS, const char *RHS) {
  while(*LHS != 0 && *RHS != 0) {
    if(*LHS != *RHS)
      return false;
    ++LHS;
    ++RHS;
  }
  return *LHS == 0 && *RHS == 0;
}

#line 3 "foo_bar.h"
static_assert(is_equal(__builtin_FILE_NAME(), "foo_bar.h"));
#line 4 "parent/foo_bar.h"
static_assert(is_equal(__builtin_FILE_NAME(), "foo_bar.h"));
