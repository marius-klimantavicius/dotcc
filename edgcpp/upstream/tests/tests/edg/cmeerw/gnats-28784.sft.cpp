//type:fp
//options:--c++20

template<typename>
constexpr bool A = true;

template<typename T, bool = A<T>>
constexpr bool B = false;

template<typename T>
struct C {
  template<class U = T> requires B<U>
  friend void f(C) { }
};
C<int> c;
