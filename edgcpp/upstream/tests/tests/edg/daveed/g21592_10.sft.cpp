//remark:constinit and destructors
//options:--c++20;fp

  struct X { int i; ~X(); };
  constinit X x{ 42 };
