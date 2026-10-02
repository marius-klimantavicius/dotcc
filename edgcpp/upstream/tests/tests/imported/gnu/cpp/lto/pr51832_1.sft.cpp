//type: fp
//options: 
# 0 "./lto/pr51832_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr51832_1.C"
# 1 "./lto/pr51832.h" 1
template<class...T> struct A
{
  static int i;
};

inline void f() { A<int>::i = 0; }
# 2 "./lto/pr51832_1.C" 2
