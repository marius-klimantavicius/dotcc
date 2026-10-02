//options_all:-r -x -tused
//options: --strict;cp

struct A { A(); ~A(); };
void f() {
  A a;
  for (;;) {
    a = A();
    continue;
    if (1) {
      a = A();
      if (0) {
        break;
      } else {
        continue;
      }
    } else {
      break;
    }
    continue;
  }  /* for */
}

