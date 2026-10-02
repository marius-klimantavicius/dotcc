//type:fn
//options:--c++26
//options_all:-A

template<class ...>
struct C {
  struct Nested { };
};

template<class ... Us>
struct S {
  template <typename ...Ts>
  friend class C<Ts>::Nested...;     // error
};
