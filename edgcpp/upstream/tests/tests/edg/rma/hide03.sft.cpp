//options_all:-r -x -tused
//options: --strict;cp

int i;
int j;
void f() {
  int i;
  {
    int i, j;
  }
}

