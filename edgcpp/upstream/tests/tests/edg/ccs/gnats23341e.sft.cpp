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
  X(const B& b, const A& a) : B(b), A(a) { printf("X(...)\n"); }
  virtual ~X() { printf("~X()\n"); }
};

struct Y : public virtual B, public virtual A {
  Y(const A& a, const B& b) : A(a), B(b) { printf("Y(...)\n"); }
  virtual ~Y() { printf("~Y()\n"); }
};

X x(B(1), A(2));
Y y(A(20), B(10));

int main() {
  printf("x.a = %d, x.b = %d\n", x.a, x.b);
  printf("y.a = %d, y.b = %d\n", y.a, y.b);
  if (x.a != 2 || x.b != 1) return 1;
  if (y.a != 20 || y.b != 10) return 2;
  return 0;
}
