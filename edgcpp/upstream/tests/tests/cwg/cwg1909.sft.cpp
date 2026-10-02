//type:fn
//options_all:--c++17 -tused -A 
template<typename> struct A {
  template<typename> struct A {};
};
void f(A<int> a) { a.~A<int>(); }

//cwg: 1909
//title: Member class template with the same name as the class
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
