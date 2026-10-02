//type:cp
//options:--c++11:--c++17
//options_all:-tused

struct A {
  ~A() { }
};

template <class T> struct B {
  B(int, A = A()) { }
};

struct C : B<int> {
  using B<int>::B;
};

template<typename> void f() {
  C c(1);
}

void g() {
  f<int>();
}
