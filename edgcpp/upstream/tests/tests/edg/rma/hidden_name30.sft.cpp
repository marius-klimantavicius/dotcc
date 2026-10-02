//options_all:-r -x -tused
//options: --strict;cp

int x, y;
void f() {
  struct x { };
  int x, y;
  struct y { };
  x = 0;
  y = 0;
  ::x = 0;
  ::y = 0;
}

