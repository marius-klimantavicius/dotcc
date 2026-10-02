//type:fp
//options:--c++ --clang_version 210100

namespace minimal
{
  int f(int);
  int i = __builtin_invoke([](auto v) -> int {
      return f(v);
    }, 1);
}

namespace function_call_operator
{
  int f(int);
  struct C {
    template<typename T>
    constexpr int operator()(T t) {
      return f(t);
    }
  };
  int i = __builtin_invoke(C{}, 1);
}
