//type: fp
//options: --c++11
// PR c++/53122
// { dg-do compile { target c++11 } }

template<typename... Args>
  void foo(Args&&...) { }

template<typename... Args>
  void bar(Args&&...)
{
  auto fn = foo<Args...>;
}
