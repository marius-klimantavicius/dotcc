//type: s
//options: --c++20
# 0 "./cpp2a/nontype-class50a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/nontype-class50a.C"




# 1 "./cpp2a/nontype-class50.C" 1



template<class T> struct X { T t; };

template<X> void f();

template<class T>
void g() {
  f<X{T{0}}>();
}

template void g<int>();
# 6 "./cpp2a/nontype-class50a.C" 2
