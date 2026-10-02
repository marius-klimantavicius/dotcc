//options_all:--c++20 -tused
  template<class T, class V>
  struct S { S(T); };

  template<class U>
  struct A {
    template<class T> using X = S<T, U>;
    template<class T> using Y = S<T, int>;
    void f() {
      new X(1);    // dependent
      new Y(1);    // not dependent
    }
  };

//cwg: 2600
//title: Type dependency of placeholder types
//meeting: Kona 11/23
//edg_status: Passes
