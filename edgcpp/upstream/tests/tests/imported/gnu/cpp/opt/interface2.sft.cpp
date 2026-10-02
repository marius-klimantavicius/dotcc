//type: fp
//options: 
# 0 "./opt/interface2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/interface2.C"






# 1 "./opt/interface2.h" 1
#pragma interface

template<class T>
struct C
{
  explicit C(const T& t) : a(t) { }
  virtual ~C() { }
  T a;
};
# 8 "./opt/interface2.C" 2

struct A
{
  A() { }
  virtual ~A() { }
};

int main()
{
  A a;
  C<A> c(a);
}
