//type:fp
//options_all:--c++17 -tused -A
  template<typename T> struct S : T {
    auto f() -> decltype(this->x);
  };

//cwg: 1836
//title: Use of class type being defined in trailing-return-type
//meeting: Toronto 7/17
//edg_status: Passes
