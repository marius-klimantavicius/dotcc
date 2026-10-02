//type:cp
//options:--c++17

struct A {
private:
  A(int);
  friend void f();
};

struct C : A {
  using A::A;
};

void f() {
  C c(0);
}
