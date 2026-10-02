//type:fn
//options_all:--c++23
  template<bool B> struct X {
    void f(short) requires B;
    void f(long);
    template<typename> void g(short) requires B;
    template<typename> void g(long);
  };
  void test() {
    &X<true>::f;      // error: ambiguous; constraints are not considered
    &X<true>::g<int>; // error: ambiguous; constraints are not considered
  }

//cwg: 2918
//title: Consideration of constraints for address of overloaded function
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27755
