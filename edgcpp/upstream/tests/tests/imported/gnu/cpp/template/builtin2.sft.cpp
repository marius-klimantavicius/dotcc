//type: fp
//options: --c++11
# 0 "./template/builtin2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./template/builtin2.C"




# 1 "./template/builtin1.C" 1


template<bool> struct cond;

template<int> struct S {
  void f(int i) {
    cond<__builtin_constant_p(i)>();
  }
};

S<1> s;
# 6 "./template/builtin2.C" 2
