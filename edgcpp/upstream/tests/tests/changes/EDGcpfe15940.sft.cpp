//type:fp
//options_all:--c++11
//remark:[4.10.1] Variadic arguments to alignas
// 1/28/15  [EDGcpfe/15940]
//
// Variadic arguments to alignas
//
// The front end now permits variadic template parameter expansion in alignas
// constructs.  The C++11 standard describes syntax for this, but failed to
// provide semantics (that is Core issue 1706).  Recent committee discussions
// indicate that the semantics should be equivalent to repeating the construct
// with individual arguments and that appears to be existing practice.
template<typename...T> struct S {
  alignas(T...) char buffer[100];  // Now accepted.
};
S<int, long, float> s;
