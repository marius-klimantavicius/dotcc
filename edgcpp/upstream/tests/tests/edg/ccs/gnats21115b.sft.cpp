//type:fn
//options:--c++14:--c++17

struct B;
extern B b;
struct B {
  B();
  B(const B &, B = b);
};
