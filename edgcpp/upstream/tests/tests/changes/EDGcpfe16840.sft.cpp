//type:fp
//options_all:--c++11 --g++
//remark:[4.11] GNU compatibility: alignas on a typedef
// 4/14/16  [EDGcpfe/16840]
//
// GNU compatibility: alignas on a typedef
//
// The C++ standard does not allow alignas on a typedef, but GNU accepts it.
// The front end now gives a warning instead of an error on such cases.
typedef int A alignas(8);
typedef int B __attribute__((aligned(8)));
A a;
B b;
static_assert(alignof(a) == 8, "Oops");
static_assert(alignof(b) == 8, "Oops");
