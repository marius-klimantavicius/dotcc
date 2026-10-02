//type:fp
//options_all:--c++17 --clang_version=160000 -tused -w
//remark:[6.8] Partial-ordering of conditionally-explicit member function template
// 3/12/25  [EDGcpfe/27847,EDGcpfe/27867,EDGcpfe/27894]
//
// Partial-ordering of conditionally-explicit member function template
//
// The front end previously often failed to establish partial ordering of member
// templates when one of the templates is conditionally-explicit.
//
// In this example, the first constructor is "more specialized" than the second,
// but the front end failed to establish that because of the presence of the
// dependent conditional "explicit" specifier.  That is now fixed.
template<typename> struct F { static const bool b = false; };
template<typename> struct U;
struct S {
  template<int = 0> S(int);  // (1)
  template<typename ...Ts> explicit(F<U<Ts...>>::b) S(Ts...);
};
S s(42);  // Previously ambiguous.  Now selects (1).
