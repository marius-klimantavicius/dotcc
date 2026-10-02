//type: fn
//options: 
struct A {
};

template <typename T>
struct S : public A {
  using A::operator(); // { dg-error "has not been declared" }
};
