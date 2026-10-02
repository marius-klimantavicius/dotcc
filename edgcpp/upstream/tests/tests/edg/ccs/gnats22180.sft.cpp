//type:cp
//options::--gnu_version 50500
//options_all:--c++11 -w

namespace std
{
  template<typename>
  struct remove_reference;

  template<typename _Tp>
  constexpr _Tp&&
  forward(typename std::remove_reference<_Tp>::type& __t)
    noexcept
  { return static_cast<_Tp&&>(__t); }
};

class CCC
{
public:
  template<typename T, typename... Args>
  void fn(T val, Args &&... args)
  {
    T tmp(val, std::forward<Args>(args)...);
  }
};

template<typename T, typename... Args>
void fn2(T val, Args &&... args)
{
  T tmp(val, std::forward<Args>(args)...);
}

int main()
{
  CCC instance;
  instance.fn(0);
  fn2(1);
  return 0;
}
