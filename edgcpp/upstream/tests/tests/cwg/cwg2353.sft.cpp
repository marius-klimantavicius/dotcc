//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
struct X {
    static const int n = 0;
  };
  X x = {};

  int b = x.n;

//cwg: 2353
//title: Potential results of a member access expression for a static data member
//meeting: Kona 02/19
//edg_status: Passes
