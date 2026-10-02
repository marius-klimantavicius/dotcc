//type:fn
//options_all:--c++20 -tused -A
  template <typename T>
  struct Fish { static const bool value = true; };

  struct Other {
    int p();
    auto q() -> decltype(p()) *;
  };

  class Outer {
    // The following declares a member function of class Other.
    friend auto Other::q() -> decltype(this->p()) *;
  };

//cwg: 1837
//title: Use of this in friend and local class declarations
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23837
