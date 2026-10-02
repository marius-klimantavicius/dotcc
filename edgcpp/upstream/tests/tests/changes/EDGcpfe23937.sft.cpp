//type:fp
//options_all:--microsoft_v 1928
//remark:[6.3] Microsoft compatibility: Packing and explicitly-aligned types
// 4/14/21  [EDGcpfe/23937]
//
// Microsoft compatibility: Packing and explicitly-aligned types
//
// In Microsoft modes, the front end ignores packing directives for fields of
// types with explicit alignment requirements (see the entry of 8/1/05).  Now,
// that behavior is extended to base classes and to types containing subobjects
// with explicit alignment requirements.
#pragma pack(1)
struct alignas(16) A {};
struct B { A a; };  // 16-byte aligned despite #pragma in effect.
struct D: B {};     // Ditto.
static_assert(alignof(D) == 16, "");  // Now accepted in Microsoft mode.
