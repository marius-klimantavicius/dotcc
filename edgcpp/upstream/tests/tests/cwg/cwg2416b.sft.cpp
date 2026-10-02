//type:fp
//options_all:--c++20 -tused -A -w
//
template<class T> void f(T) { /* ... */ }
template<class T> consteval T g(T) { /* ... */ }
template<> consteval void f<>(int) { /* ... */ } // OK:consteval
template<> int g<>(int) { /* ... */ } // OK: not consteval
