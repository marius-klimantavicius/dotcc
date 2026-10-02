//type:fp
//options:--c++20 -w

namespace minimal
{
  template <typename ... T> constexpr int f(T ... t) {
    return [...c = t] {
      return [c...] {
        return (c + ...);
      }();
    }();
  }
  static_assert(f(1, 2) == 3);
}

namespace explicit_value_captures
{
  template <typename ...T> constexpr int nested(T ...t) {
    return [...a = t] {
      return [a...] {
        return (a + ...);
      }();
    }();
  }

  static_assert(nested(1) == 1);
  static_assert(nested(1, 2L) == 3);
  static_assert(nested(1, (short) 2, 3L) == 6);

  static_assert(nested(1, 2) == 3);
  static_assert(nested(1, 2, 3) == 6);
}

namespace explicit_reference_captures
{
  template <typename ...T> constexpr int nested(T ...t) {
    return [...a = t] {
      return [&a...] {
        return (a + ...);
      }();
    }();
  }

  static_assert(nested(1) == 1);
  static_assert(nested(1, 2L) == 3);
  static_assert(nested(1, (short) 2, 3L) == 6);

  static_assert(nested(1, 2) == 3);
  static_assert(nested(1, 2, 3) == 6);
}

namespace implicit_value_captures
{
  template <typename ...T> constexpr int nested(T ...t) {
    return [...a = t] {
      return [=] {
        return (a + ...);
      }();
    }();
  }

  static_assert(nested(1) == 1);
  static_assert(nested(1, 2L) == 3);
  static_assert(nested(1, (short) 2, 3L) == 6);

  static_assert(nested(1, 2) == 3);
  static_assert(nested(1, 2, 3) == 6);
}

namespace implicit_reference_captures
{
  template <typename ...T> constexpr int nested(T ...t) {
    return [...a = t] {
      return [&] {
        return (a + ...);
      }();
    }();
  }

  static_assert(nested(1) == 1);
  static_assert(nested(1, 2L) == 3);
  static_assert(nested(1, (short) 2, 3L) == 6);

  static_assert(nested(1, 2) == 3);
  static_assert(nested(1, 2, 3) == 6);
}

namespace multi_level_explicit_value_captures
{
  template <typename ...T> constexpr int nested(T ...t) {
    return [...a = t] {
      return [a...] {
        return [a...] {
          return (a + ...);
        }();
      }();
    }();
  }

  static_assert(nested(1) == 1);
  static_assert(nested(1, 2L) == 3);
  static_assert(nested(1, (short) 2, 3L) == 6);

  static_assert(nested(1, 2) == 3);
  static_assert(nested(1, 2, 3) == 6);
}

namespace multi_level_implicit_value_captures
{
  template <typename ...T> constexpr int nested(T ...t) {
    return [...a = t] {
      return [=] {
        return [=] {
          return (a + ...);
        }();
      }();
    }();
  }

  static_assert(nested(1) == 1);
  static_assert(nested(1, 2L) == 3);
  static_assert(nested(1, (short) 2, 3L) == 6);

  static_assert(nested(1, 2) == 3);
  static_assert(nested(1, 2, 3) == 6);
}

namespace expansion
{
  template<typename ... T>
  auto foo(T ... t) {
    return [... c = t](auto fn) {
      fn(c...);
    };
  };

  void bar() {
    auto l = foo("", 1);
    l([] (auto a, auto b) { *a; });
  }
}
