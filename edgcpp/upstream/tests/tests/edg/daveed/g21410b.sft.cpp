//remark:Singleton braced CTAD
//options:--c++17;fp

  #include <initializer_list>
  template<typename T> struct X {
    X(std::initializer_list<T>);
  };
  X<int> x{};
  X y = { x };  // Previously, y had type X<X<int>>.  Now y has type X<int>.

