//remark:Nonreal friends and SSEs
//options:--c++17 --g++;fp

template<typename T> struct S: public T::template N<S<T>> {
  friend typename T::template N<S>;
};
