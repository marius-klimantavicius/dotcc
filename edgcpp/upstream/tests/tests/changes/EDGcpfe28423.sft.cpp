//type:fp
//options_all:--c++17
//remark:Spurious error on fold expression in disambiguating prescan
// 5/27/26  [EDGcpfe/28423]
//
// Spurious error on fold expression in disambiguating prescan
//
// Previously, a unary right fold could result in a spurious error during a
// disambiguating prescan.
template<auto ... Is>
decltype((Is + ...)) f();  // Previously a spurious error, now okay.
int i = f<1>();
