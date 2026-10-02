//type:fp
//options_all:--c++20
//remark:Pack expansion in CTAD for alias templates
// 7/8/26   [EDGcpfe/28924]
//
// Pack expansion in CTAD for alias templates
//
// Previously, the front end sometimes incorrectly expanded packs for dependent
// sizeof..., fold, or pack indexing expressions when transforming deduction
// guides for alias templates.  This could result in class template argument
// deduction (CTAD) for alias templates deducing the wrong type.
// with --c++20:
template<typename, typename> constexpr bool is_same_v = false;
template<typename T>         constexpr bool is_same_v<T, T> = true;
template<typename T, int N>
struct C {
  C(T*, auto ...);
};
template<typename T, typename... Us>
C(T*, Us ...) -> C<T, sizeof ... (Us)>;
template<typename T, int N>
using A = C<T, N>;
A a{"", 1, 2, 3};
static_assert(is_same_v<decltype(a), A<const char, 3>>, "Unexpected");
  // Previously failed, now okay.
