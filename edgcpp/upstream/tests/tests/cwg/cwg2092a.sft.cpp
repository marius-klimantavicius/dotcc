//type:fn
//options_all:--c++17 -tused -A
template <class T = int> void foo(T*);

  void test()
  {
    foo(0);   // #1 valid?
  }

//cwg: 2092
//title: Deduction failure and overload resolution
//meeting: Jacksonville 2/18
//edg_status: Passes
