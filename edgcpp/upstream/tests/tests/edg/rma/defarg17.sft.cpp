//options_all:-r -x -tused  --diag_warning=949
//options: --strict;cn:;ln

typedef void F(int,int,int);
F x;
typedef void F(int,int,int=0);
F y;
typedef void F(int,int=0,int);
F z;
main() {
  x(0,0,0);
//  x(0,0);
//  x(0);
  y(0,0,0);
//  y(0,0);
//  y(0);
  z(0,0,0);
//  z(0,0);
//  z(0);
}

