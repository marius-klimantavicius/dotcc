//remark:Cast to class template being defined
//options:--c++11 --gnu=80100;fp

  template<typename> struct S {
    static constexpr S sm = S();
  };
