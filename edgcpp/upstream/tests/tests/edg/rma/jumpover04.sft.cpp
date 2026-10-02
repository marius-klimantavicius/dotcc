//options_all:-r -x -tused
//options: --strict;cn:;cp

void f(int n) {
  if (n) goto L2;
  for (int i = 10; i > 0; --i) {
L2:;
  }
}

