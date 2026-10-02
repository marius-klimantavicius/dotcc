//type:fp
//options_all:--gnu=90300
//remark:[6.3] Segfault in mark_entry
// 7/15/21  [EDGcpfe/24415]
//
// Segfault in mark_entry
//
// A segfault in mark_entry had occurred when mangling certain abi_tags
// (involving alias templates for function return types).
// --gnu_version 90300):
namespace std {
  inline namespace __cxx11 __attribute__((__abi_tag__("cxx11"))) {}
}
template <class T> using X = T(int);
struct S {
  friend X<void> f;
};
