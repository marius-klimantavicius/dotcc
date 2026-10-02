//type:fn
//options_all:--c++17 -A -tused
  void *p;
  void (*pf)();
  auto x = true ? p : pf;

//cwg: 2206
//title: Composite type of object and function pointers
//meeting: Kona 2/17
//edg_status: Passes
