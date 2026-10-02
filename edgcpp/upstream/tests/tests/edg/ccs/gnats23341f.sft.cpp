//type:rp
//options_all:--c++14

extern "C" int printf(const char*, ...);

struct A {
  A() { printf("A()\n"); }
  A(int x) : a(x) { printf("A(int)\n"); }
  virtual ~A() { printf("~A()\n"); }
  int a = 100;
};

struct B {
  B() { printf("B()\n"); }
  B(int x) : b(x) { printf("B(int)\n"); }
  virtual ~B() { printf("~B()\n"); }
  int b = 200;
};

struct X : public virtual A, public virtual B {
  X() : B(B(1)), A(A(2)) { printf("X(...)\n"); }
  virtual ~X() { printf("~X()\n"); }
};

struct Y : public virtual B, public virtual A {
  Y() : A(A(20)), B(B(10)) { printf("Y(...)\n"); }
  virtual ~Y() { printf("~Y()\n"); }
};

X x;
Y y;

int main() {
  printf("x.a = %d, x.b = %d\n", x.a, x.b);
  printf("y.a = %d, y.b = %d\n", y.a, y.b);
  if (x.a != 2 || x.b != 1) return 1;
  if (y.a != 20 || y.b != 10) return 2;
  return 0;
}
