//type:fn 
//options_all:--c++17 -tused -A 
//
  template<typename, typename = decltype(sizeof(0))> struct X;
  template<typename T> struct X<T, decltype(sizeof(T()))> {
    typedef int type;
    void f() {
      X<T, decltype(sizeof(T()))>::type m; // ok, current instantiation
      X<T, decltype(sizeof(int))>::type n; // error, not the current instantiation, need decltype
    }
  };

//cwg: 2064
//title: Conflicting specifications for dependent decltype-specifiers
//meeting: Jacksonville 2/16
//edg_status: Passes
