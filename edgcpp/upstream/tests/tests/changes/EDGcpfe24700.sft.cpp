//type:fp
//options_all:--c++20
//remark:[6.6] Spurious failure of or-ed constraints
// 10/11/23 [EDGcpfe/24700,EDGcpfe/26716]
//
// Spurious failure of or-ed constraints
//
// This previously failed due to incorrect logic in handling or-ed constraints.
// That is now fixed.
template<typename T> concept C = T::value || true;
static_assert(C<int>);
