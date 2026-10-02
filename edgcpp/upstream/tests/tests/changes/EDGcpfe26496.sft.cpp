//type:fp
//options_all:--ms_c++20 --microsoft_v 1940
//remark:[6.6] __is_layout_compatible and qualified array types
// 7/14/23  [EDGcpfe/26496]
//
// __is_layout_compatible and qualified array types
//
// The front end previously failed to ignore qualifiers on array types when
// evaluating the Microsoft-mode __is_layout_compatible intrinsic.  That is now
// fixed.
static_assert(__is_layout_compatible(const int[3], int[3]));
  // Previously failed.  Now okay.
