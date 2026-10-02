//options_all:-r -x -tused
//options: --strict;cn:;cp

void g(int i) { }
void f(int i, int j) {
  if (i) goto L1;
  {
    int b[2] = { 0, 0 };
L1: g(b[0]);
  }
  if (i) goto L2;
  {
    int a[2] = { i, j };
L2: g(a[0]);
  }
  if (i) goto L3;
  {
    int a[2] = { i, j };
    int b[2] = { 0, 0 };
L3: g(a[0] + b[0]);
  }
  if (i) goto L4;
  {
    int b[2] = { 0, 0 };
    int c[2] = { i, j };
L4: g(b[0] + c[0]);
  }
  if (i) goto L5;
  {
    int a[2] = { i, j };
    int b[2] = { 0, 0 };
    int c[2] = { i, j };
L5: g(a[0] + b[0] + c[0]);
  }
}

