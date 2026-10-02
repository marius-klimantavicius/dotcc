//type:fn
//options:--c++ --clang_version 210100

namespace too_few_args
{
  struct C
  {
    void f();
  };

  void g(int, int);

  void f()
  {
    __builtin_invoke();         // error
    __builtin_invoke(&C::f);    // error
    __builtin_invoke(g);        // error
    __builtin_invoke(g, 1);     // error
    __builtin_invoke(g, 1, 2, 3); // error
  }
}
