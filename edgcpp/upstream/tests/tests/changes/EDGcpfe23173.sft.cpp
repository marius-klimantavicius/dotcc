//type:fp
//options_all:--c++17
//remark:[6.5] Spurious error on non-compound discarded statements
// 11/28/22 [EDGcpfe/23173,EDGcpfe/24085,EDGcpfe/25551,EDGcpfe/25750]
//
// Spurious error on non-compound discarded statements
//
// The front-end failed to correctly skip over non-compound discarded statements
// containing braces during instantiation.
auto v = [](auto i) {
  if constexpr (false) int{-1}*i;  // Previously a spurious error.  Now okay.
  return 0;
} (0);
