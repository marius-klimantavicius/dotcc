//type: fp
//options: 
# 0 "./lto/devirt-30_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-30_0.C"



# 1 "./lto/../ipa/devirt-30.C" 1
# 9 "./lto/../ipa/devirt-30.C"
struct A
{
  virtual void f() = 0;
  virtual ~A();
};

struct B : A
{
  virtual ~B() {}
};

void f(B* b)
{
  delete b;
}
# 5 "./lto/devirt-30_0.C" 2
