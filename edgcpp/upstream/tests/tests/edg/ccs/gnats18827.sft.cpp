//type:cp
//options::--microsoft:--g++:--clang
//options_all:--c++11

enum class E {
  a,
  b
};

struct Wrap
{
  explicit operator __underlying_type(E)();
};

template<typename T>
Wrap operator|(T, T);

void f()
{
  static_cast<int>(E::a | E::b);
}
