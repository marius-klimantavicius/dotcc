//type:fn
//options_all:--c++17

struct A {
  union {
    int x;
  };
};

struct B {
  A val = A ( A {A()} );
  struct A {
    union { int x = 37 ; };
  };
};
