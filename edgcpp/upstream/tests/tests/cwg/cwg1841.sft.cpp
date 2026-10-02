//type:fp
//options_all:--c++20 -tused -A
template<int N> struct C { };

struct B : C<1> {
  bool f() { return new C<0>; }
};

//cwg: 1841
//title: < following template injected-class-name
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23848
