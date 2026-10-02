//type:fn
//options:--c++11:--c++17

struct A {};
struct B {
  A val = C();
  struct C {
    union { int x = 37; };
  };
};
