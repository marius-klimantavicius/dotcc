//type:fp
//options:--c++11:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  struct A {
    int i;
  };
  template<typename ... Ts>
  decltype(A{Ts{} ...}) f();
  auto v = f<int>();
}

namespace braced_aggr_init
{
  struct A
  {
    int i;
  };

  template<typename ... Ts>
  decltype(A{Ts{} ...}) f();

  auto v = f<int>();
}

namespace braced_ctor_init
{
  struct A
  {
    A();
    A(int);
  };

  template<typename ... Ts>
  decltype(A{Ts{} ...}) f();

  auto v = f<int>();
}

namespace parenthesized_ctor_init
{
  struct A
  {
    A();
    A(int);
  };

  template<typename ... Ts>
  decltype(A(Ts{} ...)) f();

  auto v = f<int>();
}

namespace aggregate_init_needed
{
  struct Y
  {
    Y(int);
  };

  struct S
  {
    int x;
    Y y;
  };

  template<typename ... Ts>
  decltype(S{Ts() ...}) f(Ts ... ts);

  auto v = f<int>(1, 2);
}
