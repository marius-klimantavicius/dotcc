//type:fp
//options_all:--ms_c++17
//remark:[6.8] Microsoft compatibility: Packing and explicitly-aligned types
// 7/10/25  [EDGcpfe/28308]
//
// Microsoft compatibility: Packing and explicitly-aligned types
//
// In Microsoft modes, the front end ignores packing directives for subobjects of
// types with explicit alignment requirements (see the entry for EDGcpfe/23937).
// However, this behavior did not extend to indirect base classes containing
// subobjects with explicit alignment requirements.
#pragma pack(1)
struct alignas(16) A {};
struct B { A a; };  // 16-byte aligned despite #pragma in effect.
struct D : B {};    // Ditto.
struct E : D {};    // Ditto.
static_assert(alignof(E) == 16, "");  // Now accepted in Microsoft mode.
