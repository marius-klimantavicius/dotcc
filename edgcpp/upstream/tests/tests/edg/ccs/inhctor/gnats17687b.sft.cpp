//type:rp
//options::-DNEG;fn
//options_all:--c++17 -tused

extern "C" int printf(const char *, ...);

struct B1 {
  B1(int x, ...) { printf("B1 called, first arg: %d\n", x); }
};

struct B2 {
  B2(double) { }
};

int get() { return 13; }

struct D1 : B1 {
  using B1::B1;  // inherits B1(int, ...)
  int x;
  int y = get();
};

void test() {
  D1 d(2, 3, 4); // OK: B1 is initialized by calling B1(2, 3, 4),
                 // then d.x is default-initialized (no initialization is performed),
                 // then d.y is initialized by calling get()
#ifdef NEG
  D1 e;          // error: D1 has no default constructor
#endif
}

struct D2 : B2 {
  using B2::B2;
  B1 b;
};

#ifdef NEG
D2 f(1.0);       // error: B1 has no default constructor
#endif

struct W { W(int) { printf("W called\n"); } };
struct X : virtual W { using W::W; X() = delete; };
struct Y : X { using X::X; };
struct Z : Y, virtual W { using Y::Y; };
Z z(0); // OK: initialization of Y does not invoke default constructor of X

template<class T> struct TA : T {
  using T::T;    // inherits all constructors from class T
  ~TA() { }
};

TA<D1> ta(10);

int main() {
  test();
}
