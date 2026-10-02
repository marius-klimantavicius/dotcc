//options_all:-r -x -tused
//options: --strict;cp

void f(int i) {
  if (i) {
  } else if (i+1) {
    if (i+2) {
      goto L;
    }
  }
L:;
}


