//remark:C++11 constexpr constraint
//options:--gnu=59999 --c++11 --no_defer;fp:--c++11;fn

template <typename T> class moo
{
public:
  constexpr bool quack() const noexcept {}
};

int main(void)
{
  moo<int> w;
  return 0;
}
