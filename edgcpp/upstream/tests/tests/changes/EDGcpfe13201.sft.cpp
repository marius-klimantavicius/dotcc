//type:fp
//options_all:--c++11
//remark:[4.6] Proxy class for template parameters used in pointer-to-members
// 1/29/13  [EDGcpfe/13201,EDGcpfe/13202]
//
// Proxy class for template parameters used in pointer-to-members
//
// In cases like "int (T::*)()", where T is a template parameter, the template
// parameter T should be replaced with a proxy class, but hadn't been, leading to
// mangled names that had "?" characters.  Additionally, a change was made to the
// substitutions mechanism in the IA-64 ABI to prevent proxy classes from being
// mangled twice (leading, in some configurations to an "alloc_substitution:
// missed mangling substitution" assertion failure).  The mangling change
// is contingent on ABI_COMPATIBILITY_VERSION >= 406.
template <class Z> using B = int Z::X::*;
template <class... T> void f(B<decltype(T())>... args);
int main() {
  f();
}
