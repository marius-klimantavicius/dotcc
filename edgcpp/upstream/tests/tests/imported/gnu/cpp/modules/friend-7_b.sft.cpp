//type: fp
//options:  --c++20 --modules
# 0 "./modules/friend-7_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/friend-7_b.C"

# 1 "./modules/friend-7.h" 1
template<class T>
struct A {
  template<class U> struct B { };
  template<class U> friend struct B;
};
# 3 "./modules/friend-7_b.C" 2
import "friend-7_a.H";

A<int> a;
A<int>::B<char> b;
