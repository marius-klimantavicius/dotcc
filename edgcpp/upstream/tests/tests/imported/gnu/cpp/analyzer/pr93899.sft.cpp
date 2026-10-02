//type: fp
//options: --c++11
# 0 "./analyzer/pr93899.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr93899.C"

# 1 "./analyzer/../abi/mangle55.C" 1



struct A { int i; };

template <class T, class U> auto f1 (U u, T U::* p) -> decltype(u.*p) { return u.*p; }

template <class T, class U> auto f2 (U* u, T U::* p) -> decltype(u->*p) { return u->*p; }

int main()
{
  A a = {};
  f1(a, &A::i);
  f2(&a, &A::i);
}
# 3 "./analyzer/pr93899.C" 2
