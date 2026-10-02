//type:fp
//options:--c++20
//options_all:-A

int glob;
struct A {
  constexpr ~A() { p = &glob; }
  int *p;
};
constexpr int f() {
  typedef const A &AR;
  const A &ref = AR{0};
  delete ref.p;
  return 0;
}
extern constexpr int x = f(); // okay

//cwg: 2941
//title: Lifetime extension for function-style cast to reference type
//meeting: Kona 11/25
//edg_status: Passes
