//options_all:-r -x -tused
//options: --strict;cp

int f(int i, int j) {
  i = 0;
  j = 1;
L1:
  i = i+j;
  if (i < 5) {
    goto L1;
  }
L2:
  int k = i;
  return k;
}

