//options_all:-r -x -tused --diag_suppress=102
//options: --strict;cn:;ln

extern enum E1 x[10];
extern enum E1 y[10][10];
enum E1 { a,b,c };
main() {
  x[0] = (E1)0;
  y[0][0] = (E1)0;
}


