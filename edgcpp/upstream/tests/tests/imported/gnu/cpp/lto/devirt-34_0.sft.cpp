//type: fp
//options: 
# 0 "./lto/devirt-34_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-34_0.C"



# 1 "./lto/../ipa/devirt-34.C" 1


struct A {virtual int t(){return 42;}};
struct B:A {virtual int t(){return 1;}};

struct A aa;
struct B bb;
int
t(struct B *b)
{
  struct A *a=b;
  a->t();

  return 0;
}
# 5 "./lto/devirt-34_0.C" 2
