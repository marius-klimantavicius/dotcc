//options_all:-r -x -tused
//options: --strict;cp:--diag_suppress=172;cp

extern const int i = 1;
const int j = 0;
extern "C" const int k = 2;
extern "C" {
  extern const int z = 1;
  const int q = 3;
}
void f() {
  extern const int x, y;
}
const int x = 0;
extern const int y = 0;

