//options_all:--c++20 -A
  template<class T>
  void f()
  {
    struct Y {
      using type = int;
    };
    Y::type y;  
  }

//cwg: 2936
//title: Local classes of templated functions should be part of the current instantiation
//meeting: Wroclaw 11/24
//edg_status: Passes
