//type:fn
//options_all:--c++23
 template<class> struct A { struct B { void c(int); }; };
  template<class T> void A<T>::B::c(int = 0) {}

//cwg: 2683
//title: Default arguments for member functions of templated nested classes
//meeting: Varna 6/23
//edg_status: Passes
