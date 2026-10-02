//type:fp
//options_all:--c++17 -tused -A
  template<int &> struct X;
  template<int &N> void f(X<N>&);
  int n;
  void g(X<n> &x) { f(x); }

//cwg: 2091
//title: Deducing reference non-type template arguments
//meeting: Oulu 6/16
//edg_status: Passes
