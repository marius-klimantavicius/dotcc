//remark:GCC overload disambiguation
//options:--c++11 --gnu=60400;fn:--c++11 --gnu=70000;fp

  template<typename> struct B {
    template<typename T> void f(T);
  };
 
  struct D: public B<D> {
    using B<D>::f;
    template<typename T> int f(T);
  } d;
  auto r = d.f(42);
