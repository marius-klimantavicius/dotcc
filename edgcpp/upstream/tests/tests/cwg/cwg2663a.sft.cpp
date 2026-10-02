//options:--c++23 -A
struct C {
  int i;
};

struct D1 : C { };
struct D2 : C { };

struct D3 : D1, D2 {
  using D1::i;   // OK, equivalent to using C::i
};

//cwg: 2663
//title: Example for member redeclarations with using-declarations
//meeting: Varna 6/23
//edg_status: EDGcpfe/23847
