//type: fp
//options: 
# 0 "./cpp0x/nolinkage1a.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/nolinkage1a.cc"
# 1 "./cpp0x/nolinkage1.h" 1
template <class T>
struct A
{
  A();
};

template <class T>
A<T>::A() { }
# 2 "./cpp0x/nolinkage1a.cc" 2

typedef struct { double d; } *BP;

void f(BP) { }

A<BP> b;

static void g()
{
  struct B { };
  A<B> a;
}

int dummy() { g(); f(0); return 0; }
