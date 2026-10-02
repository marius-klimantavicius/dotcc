//remark:CLang partial spec matching
//options:--c++14 --clang_v=70000;fn

template<int> struct I {};

template<typename> struct S;
template<int* N> struct S<I<N>> {};

int n;
S<I<&n>> s;

