//options_all:-r -x -tused
//options: --strict;cn:--microsoft_16;fp



// Test type qualifiers above function type in definition
void __far f(int);
void __far f(const int p) {}
struct A {
  void __far f(int p);
};
void __far A::f(int p) {}
typedef void __far F(float);
F ff;
typedef void F2(float);
F2 __far fff;

