//type: fp
//options: 
# 0 "./compat/break/vbase10_main.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/vbase10_main.C"





# 1 "./compat/break/vbase10.h" 1
struct A {
  virtual void f();
  char c1;
};

struct B {
  B();
  char c2;
};

struct C : public A, public virtual B {
};
# 7 "./compat/break/vbase10_main.C" 2

extern void vbase10_x (void);

int
main ()
{
  vbase10_x ();
}
