//type: lp
//options: --c++11 nolinkage1a.cc
# 0 "./cpp0x/nolinkage1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/nolinkage1.C"






# 1 "./cpp0x/nolinkage1.h" 1
template <class T>
struct A
{
  A();
};

template <class T>
A<T>::A() { }
# 8 "./cpp0x/nolinkage1.C" 2

typedef struct { int i; } *AP;

void f(AP) { }

A<AP> a;

static void g()
{
  struct B { };
  A<B> a;
}

int main() { g(); f(0); return 0; }
