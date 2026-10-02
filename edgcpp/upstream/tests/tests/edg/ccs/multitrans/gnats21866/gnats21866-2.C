template<typename>
struct B {
  enum class ENUM { i };
  virtual ENUM func();
};
B<int> b;

template<typename T>
B<T>::ENUM B<T>::func() {
  return ENUM::i;
}
