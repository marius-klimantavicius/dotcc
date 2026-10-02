//remark:CLang partial spec matching
//options:--c++14 --clang_v=70000;fp

template<int> struct I {};

template<typename> struct S;
template<short N> struct S<I<N>> {};

S<I<100>> s;

