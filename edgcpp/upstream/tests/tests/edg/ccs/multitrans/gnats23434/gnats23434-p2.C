template<typename> struct X {
  template<typename> void func();
};

template<typename T>
struct Y : public X<T> {
  using X<T>::func;
};

