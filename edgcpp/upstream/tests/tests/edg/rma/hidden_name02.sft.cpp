//options_all:-r -x -tused
//options: --strict;cp

int x;
void f() {
  int x = 1;
  ::x = x;
}

