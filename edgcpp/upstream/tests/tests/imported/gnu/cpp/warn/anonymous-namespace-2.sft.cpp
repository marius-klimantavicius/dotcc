//type: fp
//options: 
# 0 "./warn/anonymous-namespace-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/anonymous-namespace-2.C"



# 1 "./warn/anonymous-namespace-2.h" 1
namespace {
  struct bad { };
}
# 5 "./warn/anonymous-namespace-2.C" 2

namespace {
    struct good { };
}

struct g1 {
    good * A;
};
struct g2 {
    good * A[1];
};
struct g3 {
    good (*A)[1];
};
# 21 "foo.C"
struct b1 {
    bad * B;
};
struct b2 {
    bad * B[1];
};
struct b3 {
    bad (*B)[1];
};
