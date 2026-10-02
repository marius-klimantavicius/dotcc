//type:fp
//options_all:--c++26
//remark:C++26: Pack indexing (IL CHANGE)
// 4/26/26  [EDGcpfe/26823,EDGcpfe/27650]
//
// C++26: Pack indexing (IL CHANGE)
//
// The front end now supports pack indexing, which has been adopted for the
// upcoming C++26 Standard via WG21 paper P2662R3.
//
// A dependent type pack-index-specifier is represented as a trk_pack_index
// typeref, and a dependent pack-index-expr is represented as an enk_pack_index
// expression node.
template<int I, typename ... Ts>
constexpr auto f(Ts ... vs) -> Ts ... [I] {
  return vs ... [I];
}
static_assert(f<0>(1, 2L) == 1 && f<1>(1, 2L) == 2L);
