//type: fp
//options: 
# 0 "./compat/break/bitfield5_main.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/bitfield5_main.C"





# 1 "./compat/break/bitfield5.h" 1
struct A {
  virtual void f();
  int f1 : 1;
};

struct B : public A {
  int f2 : 1;
  int : 0;
  int f3 : 4;
  int f4 : 3;
};
# 7 "./compat/break/bitfield5_main.C" 2

extern void bitfield5_x (void);

int
main ()
{
  bitfield5_x ();
}
