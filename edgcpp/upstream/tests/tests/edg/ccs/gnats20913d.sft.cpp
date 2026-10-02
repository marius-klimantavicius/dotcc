//type:cp
//options::-DNEG;fn
//options_all:--c++20 -A -tused

struct A {
  int a;
  int&& r;
};

int f();
int &&rf();

template<typename T> T foo() {
  T v1{1, f()};
  T{1, f()};
  T v2(1, f());
  T(1, f());
#ifdef NEG
  T v3{1.0, 1};
  T{1.0, 1};
#endif
  T v4(1.0, 1);
  T(1.0, 1);
  T v5(1.0, rf());
  T(1.0, rf());

  return v4; // Make v4 used
}

int main() {
  foo<A>();
}
