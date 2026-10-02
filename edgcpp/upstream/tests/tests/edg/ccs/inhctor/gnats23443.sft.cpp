//type:rp
//options::-DEARLY
//options_all:--c++17 -tused

extern "C" int printf(char const*, ...);

struct A {
  ~A() { printf("~A()\n"); }
};

template <class T> struct B {
  B(int, A = A()) { }
};

struct C : B<int> {
  using B<int>::B;
};

#ifdef EARLY
C cx(2);  // To force early instantiation.
#endif

template <class T> void f() {
  C c(1);
}

void g() {
  printf("f<int>():\n");
  f<int>();
  printf("f<short>():\n");
  f<short>();
  printf("Done with g()\n");
}

int main() {
  g();
}
