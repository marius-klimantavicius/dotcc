struct X {
  template<typename> static constexpr int k = 0;
  template<int> struct Y;
  template<typename T> using A = Y<k<T>>;
  template<typename T> A<T> foo(T);
};
