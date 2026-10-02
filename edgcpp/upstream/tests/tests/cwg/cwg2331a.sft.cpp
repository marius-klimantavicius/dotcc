//type:fn
//options_all:--c++20 -tused -A
template<class D>
struct B {
  D::type x;           // #1
};

struct A {using type=int;};
struct C : A,B<C> {};  // error at #1: C::type not found

//cwg: 2331
//title: Redundancy in description of class scope
//meeting: Virtual 11/20*
//edg_status: Passes
