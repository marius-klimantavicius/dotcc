template<typename> struct X
{
  template<typename T> friend void foo(T, X);
};

template<typename> struct Y {
#ifndef BUG2
  virtual X<int> func() { return {}; }
#else
  virtual void func() { X<int>(); }
#endif
};

Y<int> var;
