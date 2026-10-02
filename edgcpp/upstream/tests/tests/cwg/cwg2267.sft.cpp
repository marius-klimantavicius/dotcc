//type: fn
//options: 
//options_all: -A --c++20 -tused -e 200 --no_wrap

  struct A { } a;
  struct B { explicit B(const A&); };
  const B &b2(a);  // error: cannot copy-initialize B temporary from A

//cwg: 2267
//title: Copy-initialization of temporary in reference direct-initialization
//meeting: Kona 02/19
//edg_status: EDGcpfe/23402
//fixed_in: 6.2
