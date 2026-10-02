//remark:Inherited initializer-list ctors
//options:--c++17;fp

#include <initializer_list>

template<typename U>
struct B {
  template<typename T>
    B(std::initializer_list<T>, U = U{});
};

template<typename T>
struct D: B<T> {
  using B<T>::B;
  D();
};

D<int> d{1, 2, 3};

