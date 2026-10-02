//remark:GNU using::operator= behavior
//options:--c++14;fn:--c++14 --gnu_version 50400;fp

template<typename, typename> struct C;
template<typename T> struct C<T, T> {
  using Type = T;
};

struct B {
  template<typename T, typename = typename C<T, int>::Type>
    int operator=(T);
};

struct D: B {
  using B::operator=;
  template<typename T, typename = typename C<T, float>::Type>
    int& operator=(T);
} d;

int r = (d = 42);

