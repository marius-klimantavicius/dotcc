//type:fp
//options_all:--ms_c++17 --microsoft_version=1942 --msvc_target_version=1942
//remark:[6.8] Microsoft compatibility: Interaction between #pragma pack and alignas
// 7/17/25  [EDGcpfe/28309]
//
// Microsoft compatibility: Interaction between #pragma pack and alignas
//
// MSVC ignores packing directives for fields of types with explicit alignment
// requirements.  The front end already emulated that when the explicit alignment
// is specified with __declspec(align(...)) (see the Changes entry of 8/1/05),
// but failed to do so when the alignment is specified using the standard
// "alignas" specifier.  That is now fixed.
struct S {
  alignas(16) double x;
  double y;
};
#pragma pack(8)
struct P {
  S x;
  bool y;
};
static_assert(sizeof(P) == 32);  // Previously failed.  Now okay.
static_assert(alignof(P) == 16); // Ditto.
