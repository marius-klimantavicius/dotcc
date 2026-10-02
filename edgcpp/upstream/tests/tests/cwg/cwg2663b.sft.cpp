//type:fn
//options:--c++23
struct C {
  int i;
};

struct D1 : C { };
struct D2 : C { };

struct D3 : D1, D2 {
  using D1::i;   // OK, equivalent to using C::i
  using D1::i;   // error: duplicate
  using D2::i;   // error: duplicate, also names C::i
};
