//options_all:-r -x -tused
//options: --strict;cn

void f();
class A { static int i; friend void f(); };
class B { static int j; };
void f() {
  A::i = 0;
  B::j = 0;
}

