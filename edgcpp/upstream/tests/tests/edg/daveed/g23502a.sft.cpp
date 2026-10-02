//remark:Static data member initializer instantiation
//options:--c++17 --clang;fp:--c++17 --gnu=70300;fp:--c++17;fn

template <class X> struct S {
  static inline int index = X::initializeIndex();
};

struct T : public S<T> {
  static int initializeIndex() { return 0;}
};
