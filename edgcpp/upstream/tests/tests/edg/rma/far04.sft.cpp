//options_all:-r -x -tused
//options: --strict;cn

void far f(int);
void far f(const int p) {}
struct A {
  void far f(int p);
};
void far A::f(int p) {}
typedef void far F(float);
F ff;
typedef void F2(float);
F2 far fff;

