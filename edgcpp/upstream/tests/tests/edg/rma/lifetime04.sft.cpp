//options_all:-r -x -tused
//options: --strict;cp

/*
struct A { A(); ~A(); };
void f1() {
  {
    goto L1;
L2:
    goto L1;
  }
L1:;
  goto L2;
  A a;
}
void f2() {
  {
    goto L1;
L2:
    goto L1;
  }
L1:;
  goto L2;
}
void f3() {
  A a;
  {
    goto L1;
L2:
    goto L1;
  }
L1:;
  goto L2;
}
void f4() {
  {
    {
      A a;
    }
    goto L1;
L2:
    goto L1;
  }
L1:;
  goto L2;
}
*/

void f(int i, int j) {
L1:
  {
    if (i) goto L1; else goto L2;
L3:;
  }
  if (i) goto L1; else
  {
    if (i) goto L3; else goto L4;
L2:;
  }
L4:;
}


