//type: fp
//options:  --c++20 --modules
# 0 "./modules/tpl-friend-15_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tpl-friend-15_b.C"




module M:b;
extern "C++" {
# 1 "./modules/tpl-friend-15.h" 1


template <typename T>
struct A {
  friend void x();
};
template <typename T>
struct B {
  virtual void f() { A<T> r; }
};
template struct B<int>;
# 8 "./modules/tpl-friend-15_b.C" 2
}
