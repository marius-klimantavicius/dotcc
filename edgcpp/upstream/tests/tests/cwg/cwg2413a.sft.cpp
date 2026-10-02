//type:fn
//options_all:--c++20 -tused -A
struct A {
  using B=int;
  A f();
};
struct C : A {};
template<class T>
void g(T t) {
  decltype(t.A::f())::B i;  // error: typename needed to interpret B as a type
}
template void g(C);         // ...even though A is ::A here
