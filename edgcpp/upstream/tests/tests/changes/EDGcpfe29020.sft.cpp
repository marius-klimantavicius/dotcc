//type:fp
//options_all:--microsoft --c++17
// 8/27/26  [EDGcpfe/29020]
//
// Microsoft mode: Out-of-range enumerator folded to 0 with a typeref enum-base
//
// In Microsoft mode an enumerator whose value is outside the range of a fixed
// underlying type is truncated into that type rather than rejected.  When the
// enum-base was expressed with a typedef name (or anything other than a bare
// built-in type name), a signed underlying type caused the truncated value to
// become 0.
//
// This is now fixed.
typedef short i16;
enum ViaTypedef : i16 { A = 0x7FFFFFFF };
static_assert(A == -1, "A");
