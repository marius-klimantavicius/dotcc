//options_all:-r -x -tused
//options: --strict;cn

class A { };
void x()
{
  struct S;
  extern S a;
  extern S f();
  extern void g(S);
}

