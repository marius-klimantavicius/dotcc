//options_all:-r -x -tused
//options: --strict;cp

struct complex {
  complex();
  complex(double);
  complex(double,double);
};
complex v[6] = { 1, complex(1,2), complex(), 2 };
struct X {
  int i;
  float f;
  complex c;
} x = { 99, 88.8, 77.7 };

