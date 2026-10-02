//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
// fails when not c++20

  struct B {
  int n = B{}.n;            // error
  };

//cwg: 2317
//title: Self-referential default member initializers
//meeting: Kona 02/19
//edg_status: Passes
