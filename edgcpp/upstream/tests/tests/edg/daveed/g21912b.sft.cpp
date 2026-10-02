//remark:Disambiguation of reversed-operand operator==
//options:--c++20;fp:--c++20 --microsoft;fp

// for EDGcpfe/23514

  template<typename T> struct S {
    template<typename U> bool operator==(S<U> const&) /*not const*/;
  };                               
  bool f() {
    S<int> s;
    return s == s;  // Previously triggered an abort.  Now okay.
  }
