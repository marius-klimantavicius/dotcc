//remark:Narrowing conversion severity
//options:--gnu=90300 --c++14;fp:--clang_v=90000 --c++14;fn

struct S { long long y; int x; };
  S g(long long p) {
    return { p, p };
  }
