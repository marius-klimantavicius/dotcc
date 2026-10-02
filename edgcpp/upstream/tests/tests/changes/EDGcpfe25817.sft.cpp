//type:fp
//options_all:--ms_c++17
//remark:[6.5] Skipping discarded substatements of constexpr if containing less-than operator
// 4/27/23  [EDGcpfe/25817]
//
// Skipping discarded substatements of constexpr if containing less-than operator
//
// In modes where non-class templates are not parsed (e.g., permissive Microsoft
// mode), the front end would sometimes fail to correctly skip the discarded
// substatements of a constexpr if during instantiation.
// --ms_c++17:
template<int> struct X { };
template<bool B>
void f() {
  if constexpr (B) {
    int X = 0;
    bool b = X < 0;
  }
}  // Previously a spurious 'expected a "}"' error.  Now okay.
template void f<false>();
