//type:fp
//options_all:--gcc
//remark:[4.14] GNU compatibility: __builtin_types_compatible_p and const array elements
// 8/25/17  [EDGcpfe/18644]
//
// GNU compatibility: __builtin_types_compatible_p and const array elements
//
// In GNU C mode with gnu_version >= 40000, the front end now ignores type
// qualifiers on array element types when evaluating the GNU intrinsic function
// __builtin_types_compatible_p (in GNU C++ mode, this was already the case).
_Static_assert(__builtin_types_compatible_p(int const[8], int[8]), "");
  // Now accepted in recent GNU C modes.
