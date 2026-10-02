//type:fp
//options_all:--c++17 -tused -A
  namespace A { namespace B { int x; } }
  namespace C { namespace B = A::B; }
  using namespace A;
  using namespace C;
  int x = B::x;

//cwg: 2218
//title: Ambiguity and namespace aliases
//meeting: Kona 2/17
//edg_status: Passes
