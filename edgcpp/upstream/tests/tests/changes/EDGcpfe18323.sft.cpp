//type:fp
//options_all:--c++11
//remark:[4.14] Pack expansion in brace-notation cast during function template substitution
// 5/4/17   [EDGcpfe/18323]
//
// Pack expansion in brace-notation cast during function template substitution
//
// The front end did not correctly handle the expansion of a parameter pack
// inside a brace-notation cast during the substitution of a function template
// declaration.
//
// Here, the expansion of "I{f<Is>()...}" failed while substituting the argument
// list "<0>" in function template g, and the call "g<0>()" was diagnosed as an
// error as a result of that failure.  This is now fixed.
struct I { I(int); };
template<int P> auto f()->int;
template<int...Is> auto g() -> decltype(I{f<Is>()...});
I i  = g<0>();
