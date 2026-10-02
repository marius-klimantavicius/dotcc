//remark:Qualified friend templ decl
//options:--c++14;fn:--c++14 --gnu=100100;fp:--c++14 --clang_v=70000;fp

  namespace N {
    //template<typename... Ts> void f(Ts &&...ps);
    void f();
  }
  struct X {
    template<class T> friend void N::f(T&&);       // (1)
    template<class T> friend void N::f(const T&);  // (2)
  };
