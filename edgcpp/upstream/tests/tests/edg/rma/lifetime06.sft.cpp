//options_all:-r -x -tused
//options: --strict;cp

struct A { A(); ~A(); };
void f() {
  int i;
  for (i = 0; i < 10; i++) {
    A a;
    if (i == 2) continue;
L:;
    if (i > 8) break;
  }  /* for */
}

