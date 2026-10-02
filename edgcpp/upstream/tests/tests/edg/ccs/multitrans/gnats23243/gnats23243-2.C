template <typename T>
class B {
  void operator-();
};

template <typename T>
class D : public B<T> {
  using B<T>::operator-;
};
