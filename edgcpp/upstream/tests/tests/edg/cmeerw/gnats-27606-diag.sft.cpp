//type:fn
//options:--c++11 --diag_warning 2361:--c++11 --diag_error 2361

# 1 "user.h" 1
namespace user
{
  template<typename T, typename U>
  auto f(T t, U u, int) -> decltype(T{u}, void());

  template<typename T, typename U>
  int f(T t, U u, long);

  double d{};
  auto v{ float{ d } };
}

# 1 "system.h" 3
namespace system
{
  template<typename T, typename U>
  auto f(T t, U u, int) -> decltype(T{u}, void());

  template<typename T, typename U>
  int f(T t, U u, long);

  double d{};
  auto v{ float{ d } };
}

# 1 "main.cpp"
int i = user::f(0.0f, 0.0, 0);
int j = system::f(0.0f, 0.0, 0);
