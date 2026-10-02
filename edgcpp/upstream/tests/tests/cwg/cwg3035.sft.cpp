//type:fn
//options: -A --c++20

static union {
  int x = [] { return 42; }();  // error
};

//cwg: 3035
//title: Lambda expressions in anonymous unions
//meeting: Croydon 3/26
//edg_status: Passes
