//options_all:-r -x -tused
//options: --strict;cp

int i = 1;
static int j = 2, k = i;
void f() {
  int i = 1, j = k;
  static int k = 2, l = i;
}

