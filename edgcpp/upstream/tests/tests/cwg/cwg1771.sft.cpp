//type:fn
//options_all:--c++20 -tused -A
template<int> struct B : A {};
namespace N {
  template<int> void B();
  int f() {return B<0>::n;}  // error: N::B<0> is not a type
}

//cwg: 1771
//title: Restricted lookup in nested-name-specifier
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23860
