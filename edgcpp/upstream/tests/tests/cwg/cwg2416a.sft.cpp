//type:fp
//options_all:--c++17 -tused -A -w
//
template<class T> void f(T) { /* ... */ }
template<class T> constexpr T g(T) { /* ... */ }
template<> constexpr void f<>(int) { /* ... */ } // OK:constexpr 
template<> int g<>(int) { /* ... */ } // OK: not constexpr

//cwg: 2416
//title: Explicit specializations vs constexpr and consteval
//meeting: Belfast 11/19
//edg_status: Passes
