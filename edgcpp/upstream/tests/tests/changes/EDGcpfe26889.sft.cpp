//type:fp
//options_all:--microsoft_v 1936 --ms_c++17
//remark:[6.7] Pointer-to-member with zero-length array member type
// 6/19/24  [EDGcpfe/26889]
//
// Pointer-to-member with zero-length array member type
//
// In Microsoft C++ modes, the front end sometimes accepts zero-length array
// types in class scopes.  Now it also accepts zero-length array types as the
// member type for a pointer-to-member type.
struct S {};
typedef int (S::*PMZ)[0];  // Now accepted in Microsoft C++ mode.
