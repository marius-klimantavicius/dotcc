//options_all:-r -x -tused
//options: --strict;cp

int f(int i) {
  static int j;
  j = i;
  return j;
}

