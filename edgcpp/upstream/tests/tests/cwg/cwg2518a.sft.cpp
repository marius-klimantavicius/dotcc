//options_all:--c++20 
  template <class T>
  void f(T t) {
    if constexpr (sizeof(T) == sizeof(int)) {
      use(t);
    } else {
      static_assert(false, "must be int-sized");
    }
  }
  
  void g(char c) {
    f(0); // OK
    f(c); // previously an error: must be int-sized
  }

//cwg: 2518
//title: Conformance requirements and #error/#warning
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/26056
//fixed_in: 6.7
