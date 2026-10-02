template<bool> struct X;
template<typename> bool var;

template<typename> class Y {
  template <typename T, X<var<T>>>
  friend bool i(Y);
};

template<typename> class Z {
  virtual void func() { Y<int>(); }
};

Z<int> foo;
