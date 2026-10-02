//options_all:-r -x -tused --diag_warning=949
//options: --strict;cn:;fp

extern "C" void printf(char *, ...);
#define NULL 0
class A {
public:
  static A*func (int = 3);
  static A*(*ptr)(int = 4);
};

A*A::func(int i)
{
  printf("I = %d\n",i);
  return (A *)NULL;
}

A*(*A::ptr)(int) = &A::func;

main()
{
  A foo;

  A::ptr();
  A::ptr(47);
}

