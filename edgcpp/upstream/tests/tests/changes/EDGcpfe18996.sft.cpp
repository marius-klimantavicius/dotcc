//type:fp
//options_all:--c++11
//remark:[5.0] Assertion failure: alloc_substitution: missed mangling substitution
// 4/30/18  [EDGcpfe/18996]
//
// Assertion failure: alloc_substitution: missed mangling substitution
//
// In IA-64 ABI configurations where EXPENSIVE_CHECKING is TRUE, an assertion
// failure ("alloc_substitution: missed mangling substitution") could occur when
// mangling certain types for which the IA-64 ABI pre-defines a substitution.
// That has been fixed.
namespace std {
  template <class _Ty> using remove_reference_t = _Ty;
  template <class> struct char_traits;
  template <class> class allocator;
  template <class _Ty> void forward(remove_reference_t<_Ty> &);
  template <class, class, class> struct basic_string {
    basic_string();
    basic_string(basic_string &&p1) { forward(p1); }
  };
  basic_string<char, char_traits<char>, allocator<char>> name() {
    return  basic_string<char, char_traits<char>, allocator<char>>();
  }
}
