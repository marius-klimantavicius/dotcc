//remark:decltype of structured binding with dependent initializer
//options:--c++17;fp

struct A {
  int member;
};

A foo(int i)
{
  return A{i};
}

template<class _Ty>
struct my_type_trait { static constexpr bool value = false; };

template <typename T>
bool my_type_trait_v = my_type_trait<T>::value;

template <typename T>
bool function(T t)
{
  auto [a] = foo(t);
  return my_type_trait_v<decltype(a)>; // internal_error in name mangling
}

int main()
{
  function(42);
  return 0;
}

