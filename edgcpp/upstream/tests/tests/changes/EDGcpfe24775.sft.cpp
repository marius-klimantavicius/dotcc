//type:fp
//options_all:--c++14
//remark:[6.5] C23, C++23 compatibility: Allow duplicate attributes
// 3/23/23  [EDGcpfe/24775,EDGcpfe/25887]
//
// C23, C++23 compatibility: Allow duplicate attributes
//
// Both C23 and C++23 now allow duplicate attributes to be specified (see
// WG14 paper N2557 and WG21 paper P2156R1).  The latter paper was approved
// as a defect report so this change applies to all C++ versions.
[[deprecated, deprecated]] int i;   // Now accepted (previously an error).
