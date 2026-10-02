//type:fp
//options_all:--c++20
//remark:[6.8] Non-type template parameter pack expansion in nested generic lambda
// 1/31/25  [EDGcpfe/27707]
//
// Non-type template parameter pack expansion in nested generic lambda
//
// Previously, the front end failed to correctly expand non-type template
// parameters in a pack expansion in a generic lambda involving template
// parameters from different template depths.
template<int ... Is>
constexpr int f() {
  return []<int ... Js>(auto p) {
    return ((Is + Js) + ...);
  }.template operator()<1, 2>(0);
};
static_assert(f<10, 20>() == 33);  // Previously failed.  Now okay.
