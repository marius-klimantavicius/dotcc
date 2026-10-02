template<typename> struct X {
  template<typename T> static T var;
};

template<typename T>
struct Y : public X<T> {
  using X<T>::var;
};

