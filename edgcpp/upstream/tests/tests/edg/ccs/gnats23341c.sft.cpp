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

template<class... Ts>
struct X : public virtual Ts... {
  X(const Ts &... ts) : Ts(ts)... { printf("X(...)\n"); }
  virtual ~X() { printf("~X()\n"); }
};

X<B, A> x(B(1), A(2));
X<A, B> y(A(20), B(10));

int main() {
  printf("x.a = %d, x.b = %d\n", x.a, x.b);
  printf("y.a = %d, y.b = %d\n", y.a, y.b);
  if (x.a != 2 || x.b != 1) return 1;
  if (y.a != 20 || y.b != 10) return 2;
  return 0;
}
