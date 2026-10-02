//type: fn
//options:  --c++17
// { dg-do compile }
// crash test - PR 7266

template <class A>
struct B {
 typedef A::C::D E;  // { dg-error "" "" { target c++17_down } }
};
