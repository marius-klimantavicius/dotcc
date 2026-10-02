//type: fp
//options: 
# 0 "./lto/devirt-14_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-14_0.C"


# 1 "./lto/../ipa/devirt-14.C" 1





class B {
public:
  virtual int foo(void)
{
  return 0;
}
};
namespace {
class A : public B {
public:
  virtual int foo(void)
{
  return 1;
}
};
}
class B a, *b=&a;

int main()
{
  if (0)
    {
    class A a;
    a.foo();
    }
  return b->foo();
}
# 4 "./lto/devirt-14_0.C" 2
