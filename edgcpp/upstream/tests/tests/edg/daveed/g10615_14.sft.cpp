//remark:Move constructors and closures
//options:--c++11 -A;rp

extern "C" int printf(char const*, ...);

struct M {
  M() { printf("M()\n"); }
  M(M&&) { printf("M(M&&)\n"); }
  M(M const&) { printf("M(M const&)\n"); }
};

template<class T> void f(T p) {
  T t(static_cast<T&&>(p));
}

int main() {
  M m;
  f([m](){});
}

