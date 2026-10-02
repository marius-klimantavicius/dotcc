//type: fp
//options: 
# 0 "./lto/devirt-13_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-13_0.C"



# 1 "./lto/../ipa/devirt-13.C" 1



namespace {
class A {
public:
  virtual int foo(void)
{
  return 0;
}
};
}
class A a, *b=&a;

int main()
{
  return b->foo();
}
# 5 "./lto/devirt-13_0.C" 2
