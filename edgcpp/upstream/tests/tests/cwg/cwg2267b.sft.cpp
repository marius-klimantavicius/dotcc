//type: fn
//options: 
//options_all: -A --c++20 -tused -e 200 --no_wrap

  struct A { } a;
  struct B { explicit B(const A&); };
  const B &b3(a);  // error: cannot copy-initialize B temporary from A
