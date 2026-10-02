//remark:Core issue 2267: Initialization of temporary
//options:--c++20;fn

  struct A { } a;
  struct B { explicit B(const A&); };
  const B &b2(a);  // error: cannot copy-initialize B temporary from A
