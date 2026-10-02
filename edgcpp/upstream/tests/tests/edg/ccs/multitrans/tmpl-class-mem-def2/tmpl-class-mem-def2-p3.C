template<int> class iv {};

template<int> struct X {
  enum { N = 0 };
  typedef iv<N> Base;
  void func();
};

X<5> var3;
