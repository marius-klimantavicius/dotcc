//type:fn

struct A{
  A(int);
};
struct B{
  B(int);
};
struct C {};

void f(A);
void f(B);

void g(C c) {
  f(1);
  f(c);
}
