//type:fp
//options:--c++20:--c++20 --gn 150200:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename ... Ts>
  struct C
  {
    C(Ts ...);
    template<typename U> requires false
    C(U u);
  };
  C c(1);
}

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace trailing_requires_clause
{
  template<typename ... Ts>
  struct C
  {
    C(Ts ...);

    template<typename U>
    C(U u) requires false;
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<int>>);
}

namespace template_and_trailing_requires_clause
{
  template<typename ... Ts>
  struct C
  {
    C(Ts ...);

    template<typename U> requires true
    C(U u) requires false;
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<int>>);
}

namespace template_and_trailing_requires_clause_reversed
{
  template<typename ... Ts>
  struct C
  {
    C(Ts ...);

    template<typename U> requires false
    C(U u) requires true;
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<int>>);
}

namespace template_requires_clause_satisfied
{
  template<typename ... Ts>
  struct C
  {
    template<typename U> requires true
    C(U u);
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<>>);
}

namespace trailing_requires_clause_satisfied
{
  template<typename ... Ts>
  struct C
  {
    template<typename U>
    C(U u) requires true;
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<>>);
}

namespace template_and_trailing_requires_clause_satisfied
{
  template<typename ... Ts>
  struct C
  {
    template<typename U> requires true
    C(U u) requires true;
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<>>);
}

namespace no_constraints
{
  template<typename ... Ts>
  struct C
  {
    template<typename U>
    C(U u);
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<>>);
}

namespace class_template_constraints
{
  template<typename ... Ts> requires true
  struct C
  {
    template<typename U> requires true
    C(U u) requires true;
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<>>);
}

namespace all_dependent_constraints
{
  template<typename ... Ts> requires (sizeof...(Ts) == 0)
  struct C
  {
    template<typename U> requires (sizeof...(Ts) == 0 && is_same_v<U, int>)
    C(U u) requires (sizeof...(Ts) == 0 && is_same_v<U, int>);
  };

  C c(1);

  static_assert(is_same_v<decltype(c), C<>>);
}

namespace constraint_requires_clause
{
  int *f(int);

  template<typename T>
  struct C
  {
    template<typename U> requires requires (U u) { *f(u); }
    C(T, U);
  };

  C c(1, 2);
}

namespace requires_in_member_init
{
  template<typename T>
  struct C
  {
    T x { requires { [] { }(); } };
  };

  static_assert(C<bool>().x);
}

namespace requires_in_member_init_nontype_param
{
  template<typename T, int I>
  struct C
  {
    T x { requires { [] { }(); } };
  };

  static_assert(C<bool, 1>().x);
}

namespace requires_in_bitfield_member_init
{
  template<typename T, int I>
  struct C
  {
    T x : I { requires { [] { }(); } };
  };

  static_assert(C<int,4>().x == 1);
}

namespace requires_clause_member_ref
{
  template<typename T, int N>
  struct C
  {
    T x = 1;
    bool b = requires { x; };
  };

  C<int, 1> c;
}

namespace fold_expression_in_constraint
{
  template<typename>
  concept X = true;

  template <typename T, typename... Ts> requires (X<T> && ... && X<Ts>)
  struct C {
    C(T t, Ts... ts);
  };

  C c{1};
}

/*
Fix: when synthesizing implicit deduction guides in
make_template_implicit_deduction_guide, copy and substitute template-head
requires clauses from the class and constructor templates onto the generated
guide (trailing requires were already handled on the routine).
*/
