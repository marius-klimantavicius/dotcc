//type:fp
//options:--c++17:--c++17 --g++:--ms_c++17:--c++23
//options_all:-tused

namespace discard_lambda
{
  template<int I>
  void foo()
  {
    if constexpr (false) [] { } ();

    if constexpr (true) [] { } ();

    if constexpr (false) [] { } ();
    else [] { } ();

    if constexpr (true) [] { } ();
    else [] { } ();

    [] (auto p) {
      if constexpr (false) [] { } ();

      if constexpr (true) [] { } ();

      if constexpr (false) [] { } ();
      else [] { } ();

      if constexpr (true) [] { } ();
      else [] { } ();
    } (1);
  }

  template void foo<1>();
}

namespace discard_if_else_if
{
  template<int I>
  int foo()
  {
    int i = 0;

    if constexpr (true) {
    } else if (i) {
    }

    [] (auto p) {
      int i = 0;

      if constexpr (true) {
      } else if (i) {
      }

      return i;
    } (1);

    return i;
  }

  template int foo<1>();
}

namespace capture_in_dependent_discarded_statement
{
  template<int I>
  void foo()
  {
    int i = 0;

    [=] (auto... args) {
      if constexpr(sizeof ... (args) <= 0)
      {
        return i + 1;
      }
      else
      {
        return i + 2;
      }
    } ();
  }

  template void foo<0>();
}

#ifdef __cpp_if_consteval
namespace discard_if_consteval
{
  template<int I>
  constexpr void foo()
  {
    if constexpr (true) {
    } else if consteval {
    } else {
    }

    if constexpr (true) {
    } else if ! consteval {
    } else {
    }

    if constexpr (true) {
    } else if not consteval {
    } else {
    }

    if constexpr (false) {
    } else if consteval {
    } else {
    }

    if constexpr (false) {
    } else if ! consteval {
    } else {
    }

    if constexpr (false) {
    } else if not consteval {
    } else {
    }

    [] (auto p) {
      if constexpr (true) {
      } else if consteval {
        } else {
      }

      if constexpr (true) {
      } else if ! consteval {
        } else {
      }

      if constexpr (true) {
      } else if not consteval {
        } else {
      }

      if constexpr (false) {
      } else if consteval {
        } else {
      }

      if constexpr (false) {
      } else if ! consteval {
        } else {
      }

      if constexpr (false) {
      } else if not consteval {
        } else {
      }
    } (1);
  }

  template void foo<1>();
}
#endif

namespace discard_nondependent
{
  struct C
  {
    static inline int value = 0;
  };

  template<bool B, typename T>
  void foo()
  {
    [=] (auto p) {
      if constexpr (true) { }
      else { return T::value; }

      if constexpr (false) { return T::value; }
      else { }
    } (1);

    [=] (auto p) {
      if constexpr (B) { }
      else { return T::value; }
    } (1);

    [=] (auto p) {
      if constexpr (!B) { return T::value; }
      else { }
    } (1);
  }

  template void foo<true, int>();
  template void foo<false, C>();
}

namespace var_decl
{
  struct C
  {
    constexpr C(int, int)
    { }
  };

  template<bool B>
  constexpr C foo()
  {
    if constexpr (B)
    {
      constexpr auto i = 1;
      return { 1, i };
    }
    else
    {
      return { -1, 0 };
    }
  }

  template C foo<false>();
  template C foo<true>();
}

namespace pragmas
{
  template<bool B>
  void foo()
  {
    if constexpr (B)
#pragma test_next_statement
      for (; false;);
    else
#pragma test_next_statement
      for (; false;);

    if constexpr (!B)
#pragma test_next_statement
      for (; false;);
    else
#pragma test_next_statement
      for (; false;);

    if constexpr (B)
#pragma test_next_statement
      for (; false;);

    if constexpr (!B)
#pragma test_next_statement
      for (; false;);
  }

  template void foo<false>();
  template void foo<true>();
}
