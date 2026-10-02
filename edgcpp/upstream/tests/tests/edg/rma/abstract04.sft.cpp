//options_all:-r -x -tused
//options: --strict;cn

struct A;
A ff1(A, A[2]);
typedef A (F)(A, A[2]);
F ff2;
struct A {
  A f1(A, A[2]);
  typedef A (F)(A, A[2]);
  virtual void f2() = 0;
  A f3(A, A[2]);
  F f4;
  static A x;
};
A x, y[10];
A ff2(A, A[2]);

