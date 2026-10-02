template<typename T>
struct Base {
  typedef int X;
  Base();
};

template <typename T>
struct Derived : Base<T>
{
  using typename Base<T>::X;
  Derived();
};
