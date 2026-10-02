//options_all:-r -x -tused
//options: --strict;cn:;cn

struct A { const int i; } x;
struct B { struct A a[2]; } y;
struct C { struct B b[3]; } z;
main() {
  y.a[0] = x;
  x = y.a[0];
  z.b[0] = y;
  y = z.b[0];
}

