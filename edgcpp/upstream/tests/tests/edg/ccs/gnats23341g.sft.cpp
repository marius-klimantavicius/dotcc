//type:rp
//options_all:--c++14

extern "C" int printf(const char*, ...);

int counter = 0;

struct A {
  A() { printf("A()\n"); }
  A(int x) : a(x) { printf("A(int)\n"); }
  virtual ~A() { printf("~A()\n"); }
  int a = 100;
};

struct B : public virtual A {
  B() { printf("B()\n"); }
  B(int x) : b(x) { printf("B(int)\n"); }
  virtual ~B() { printf("~B()\n"); }
  int b = 200;
};

template<class... Ts>
struct X : public virtual Ts... {
  X() : Ts(Ts(++counter))... { printf("X(...)\n"); }
  virtual ~X() { printf("~X()\n"); }
};

X<B, A> x;
X<A, B> y;

int main() {
  printf("x.a = %d, x.b = %d\n", x.a, x.b);
  printf("y.a = %d, y.b = %d\n", y.a, y.b);
  if (x.a != 1 || x.b != 2) return 1;
  if (y.a != 3 || y.b != 4) return 2;
  return 0;
}
