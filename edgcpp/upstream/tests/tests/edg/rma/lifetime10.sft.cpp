//options_all:-r -x -tused
//options: --strict;cp

struct A { int i; A(int); ~A(); };
void f() {
  for (A x = A(0); x.i < 10; x = A(x.i+1)) { }
  A y = A(0);
  for (; y.i < 10; y = A(y.i+1)) { }
}

