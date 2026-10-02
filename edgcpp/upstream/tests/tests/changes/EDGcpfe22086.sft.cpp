//type:fp
//options_all:--microsoft_v 1921 --ms_c++17
//remark:[6.1] Microsoft C++ compatibility: __is_standard_layout and function types
// 12/6/19  [EDGcpfe/22086]
//
// Microsoft C++ compatibility: __is_standard_layout and function types
//
// In Microsoft C++ modes, the front end now produces a true value for
// __is_standard_layout applied to a function type.
static_assert(__is_standard_layout(int (int)), "");
  // Normally an error, but now accepted in Microsoft C++ modes.
