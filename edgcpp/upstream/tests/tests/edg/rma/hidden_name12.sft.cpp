//options_all:-r -x -tused
//options: --strict;cp

struct x { };
static union {
  int x, y;
};
struct y { };
void f() {
  struct x { };
  union {
    int x, y;
  };
  struct y { };
  x = 0;
  y = 0;
  ::x = 0;
  ::y = 0;
  struct x *p1;
  struct y *p2;
  struct ::x *p3;
  struct ::y *p4;
}

