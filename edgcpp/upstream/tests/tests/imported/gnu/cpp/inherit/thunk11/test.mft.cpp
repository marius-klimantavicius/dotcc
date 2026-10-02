//type: lp
//options:  thunk11-aux.cc
# 0 "./inherit/thunk11.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./inherit/thunk11.C"





# 1 "./inherit/thunk11.h" 1
struct A
{
  A () {}
  virtual ~A () {}
};
struct B
{
  B () {}
  virtual ~B () {}
};
struct C : public A, public B
{
  virtual void foo ();
  virtual ~C () {};
};
inline void C::foo () {}
# 7 "./inherit/thunk11.C" 2

int
main ()
{
}
