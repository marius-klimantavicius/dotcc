//type:fp
//options::-DNEG;fn:--g++ -DNEG:--microsoft -DNEG
//options_all:--c++11

namespace {
  struct S {
    static int m;
    static int n;
  };
  template<typename T> struct TS {
    static int m;
    static int n;
  };
  extern int m;
  extern int n;
#ifndef NEG
  int S::m;
  template<> int TS<int>::m{};
  int m;
#endif
}
int main() {
  return m + S::m + TS<int>::m;
}
