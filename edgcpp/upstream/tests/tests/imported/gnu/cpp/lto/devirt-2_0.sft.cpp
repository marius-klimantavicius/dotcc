//type: fp
//options: 
# 0 "./lto/devirt-2_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-2_0.C"


# 1 "./lto/../ipa/devirt-2.C" 1





extern "C" void abort (void);

class A
{
public:
  int data;
  virtual int foo (int i);
  int middleman (int i)
  {
    return foo (i);
  }
};

class B : public A
{
public:
  virtual int foo (int i);
};

class C : public A
{
public:
  virtual int foo (int i);
};

int A::foo (int i)
{
  return i + 1;
}

int B::foo (int i)
{
  return i + 2;
}

int C::foo (int i)
{
  return i + 3;
}

int __attribute__ ((noinline,noclone,noipa)) get_input(void)
{
  return 1;
}

int main (int argc, char *argv[])
{
  class B b;
  int i;
  for (i = 0; i < get_input(); i++)
    if (b.middleman (get_input ()) != 3)
      abort ();
  return 0;
}
# 4 "./lto/devirt-2_0.C" 2
