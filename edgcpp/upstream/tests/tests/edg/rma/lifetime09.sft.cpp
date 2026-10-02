//options_all:-r -x -tused
//options: --strict;cp

void f() {
  for (int i = 0; i < 2; ++i) {
    if (i == 0) {
      continue;
    } else {
      break;
    }  /* if */
  }  /* for */
}

