//type: fp
//options:  --c++23
# 1 "SemaCXX/cxx20-default-compare.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/cxx20-default-compare.cpp" 2


# 1 "SemaCXX/Inputs/std-compare.h" 1



namespace std {
inline namespace __1 {


enum class _EqResult : unsigned char {
  __equal = 0,
  __equiv = __equal,
};

enum class _OrdResult : signed char {
  __less = -1,
  __greater = 1
};

enum class _NCmpResult : signed char {
  __unordered = -127
};

struct _CmpUnspecifiedType;
using _CmpUnspecifiedParam = void (_CmpUnspecifiedType::*)();

class partial_ordering {
  using _ValueT = signed char;
  explicit constexpr partial_ordering(_EqResult __v) noexcept
      : __value_(_ValueT(__v)) {}
  explicit constexpr partial_ordering(_OrdResult __v) noexcept
      : __value_(_ValueT(__v)) {}
  explicit constexpr partial_ordering(_NCmpResult __v) noexcept
      : __value_(_ValueT(__v)) {}

  constexpr bool __is_ordered() const noexcept {
    return __value_ != _ValueT(_NCmpResult::__unordered);
  }

public:

  static const partial_ordering less;
  static const partial_ordering equivalent;
  static const partial_ordering greater;
  static const partial_ordering unordered;


  friend constexpr bool operator==(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator!=(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<=(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>=(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator==(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator!=(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator<(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator<=(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator>(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator>=(_CmpUnspecifiedParam, partial_ordering __v) noexcept;

  friend constexpr partial_ordering operator<=>(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr partial_ordering operator<=>(_CmpUnspecifiedParam, partial_ordering __v) noexcept;


  constexpr bool test_eq(partial_ordering const &other) const noexcept {
    return __value_ == other.__value_;
  }

private:
  _ValueT __value_;
};

inline constexpr partial_ordering partial_ordering::less(_OrdResult::__less);
inline constexpr partial_ordering partial_ordering::equivalent(_EqResult::__equiv);
inline constexpr partial_ordering partial_ordering::greater(_OrdResult::__greater);
inline constexpr partial_ordering partial_ordering::unordered(_NCmpResult ::__unordered);
constexpr bool operator==(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ == 0;
}
constexpr bool operator<(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ < 0;
}
constexpr bool operator<=(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ <= 0;
}
constexpr bool operator>(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ > 0;
}
constexpr bool operator>=(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ >= 0;
}
constexpr bool operator==(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 == __v.__value_;
}
constexpr bool operator<(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 < __v.__value_;
}
constexpr bool operator<=(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 <= __v.__value_;
}
constexpr bool operator>(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 > __v.__value_;
}
constexpr bool operator>=(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 >= __v.__value_;
}
constexpr bool operator!=(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return !__v.__is_ordered() || __v.__value_ != 0;
}
constexpr bool operator!=(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return !__v.__is_ordered() || __v.__value_ != 0;
}

constexpr partial_ordering operator<=>(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v;
}
constexpr partial_ordering operator<=>(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v < 0 ? partial_ordering::greater : (__v > 0 ? partial_ordering::less : __v);
}

class weak_ordering {
  using _ValueT = signed char;
  explicit constexpr weak_ordering(_EqResult __v) noexcept : __value_(_ValueT(__v)) {}
  explicit constexpr weak_ordering(_OrdResult __v) noexcept : __value_(_ValueT(__v)) {}

public:
  static const weak_ordering less;
  static const weak_ordering equivalent;
  static const weak_ordering greater;


  constexpr operator partial_ordering() const noexcept {
    return __value_ == 0 ? partial_ordering::equivalent
                         : (__value_ < 0 ? partial_ordering::less : partial_ordering::greater);
  }


  friend constexpr bool operator==(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator!=(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<=(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>=(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator==(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator!=(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator<(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator<=(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator>(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator>=(_CmpUnspecifiedParam, weak_ordering __v) noexcept;

  friend constexpr weak_ordering operator<=>(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr weak_ordering operator<=>(_CmpUnspecifiedParam, weak_ordering __v) noexcept;


  constexpr bool test_eq(weak_ordering const &other) const noexcept {
    return __value_ == other.__value_;
  }

private:
  _ValueT __value_;
};

inline constexpr weak_ordering weak_ordering::less(_OrdResult::__less);
inline constexpr weak_ordering weak_ordering::equivalent(_EqResult::__equiv);
inline constexpr weak_ordering weak_ordering::greater(_OrdResult::__greater);
constexpr bool operator==(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ == 0;
}
constexpr bool operator!=(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ != 0;
}
constexpr bool operator<(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ < 0;
}
constexpr bool operator<=(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ <= 0;
}
constexpr bool operator>(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ > 0;
}
constexpr bool operator>=(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ >= 0;
}
constexpr bool operator==(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 == __v.__value_;
}
constexpr bool operator!=(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 != __v.__value_;
}
constexpr bool operator<(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 < __v.__value_;
}
constexpr bool operator<=(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 <= __v.__value_;
}
constexpr bool operator>(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 > __v.__value_;
}
constexpr bool operator>=(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 >= __v.__value_;
}

constexpr weak_ordering operator<=>(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v;
}
constexpr weak_ordering operator<=>(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return __v < 0 ? weak_ordering::greater : (__v > 0 ? weak_ordering::less : __v);
}

class strong_ordering {
  using _ValueT = signed char;
  explicit constexpr strong_ordering(_EqResult __v) noexcept : __value_(static_cast<signed char>(__v)) {}
  explicit constexpr strong_ordering(_OrdResult __v) noexcept : __value_(static_cast<signed char>(__v)) {}

public:
  static const strong_ordering less;
  static const strong_ordering equal;
  static const strong_ordering equivalent;
  static const strong_ordering greater;


  constexpr operator partial_ordering() const noexcept {
    return __value_ == 0 ? partial_ordering::equivalent
                         : (__value_ < 0 ? partial_ordering::less : partial_ordering::greater);
  }
  constexpr operator weak_ordering() const noexcept {
    return __value_ == 0 ? weak_ordering::equivalent
                         : (__value_ < 0 ? weak_ordering::less : weak_ordering::greater);
  }


  friend constexpr bool operator==(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator!=(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<=(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>=(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator==(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator!=(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator<(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator<=(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator>(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator>=(_CmpUnspecifiedParam, strong_ordering __v) noexcept;

  friend constexpr strong_ordering operator<=>(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr strong_ordering operator<=>(_CmpUnspecifiedParam, strong_ordering __v) noexcept;


  constexpr bool test_eq(strong_ordering const &other) const noexcept {
    return __value_ == other.__value_;
  }

private:
  _ValueT __value_;
};

inline constexpr strong_ordering strong_ordering::less(_OrdResult::__less);
inline constexpr strong_ordering strong_ordering::equal(_EqResult::__equal);
inline constexpr strong_ordering strong_ordering::equivalent(_EqResult::__equiv);
inline constexpr strong_ordering strong_ordering::greater(_OrdResult::__greater);

constexpr bool operator==(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ == 0;
}
constexpr bool operator!=(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ != 0;
}
constexpr bool operator<(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ < 0;
}
constexpr bool operator<=(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ <= 0;
}
constexpr bool operator>(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ > 0;
}
constexpr bool operator>=(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ >= 0;
}
constexpr bool operator==(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 == __v.__value_;
}
constexpr bool operator!=(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 != __v.__value_;
}
constexpr bool operator<(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 < __v.__value_;
}
constexpr bool operator<=(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 <= __v.__value_;
}
constexpr bool operator>(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 > __v.__value_;
}
constexpr bool operator>=(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 >= __v.__value_;
}

constexpr strong_ordering operator<=>(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v;
}
constexpr strong_ordering operator<=>(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return __v < 0 ? strong_ordering::greater : (__v > 0 ? strong_ordering::less : __v);
}

}
}
# 4 "SemaCXX/cxx20-default-compare.cpp" 2

struct Foo {
  float val;
  bool operator==(const Foo &) const;
  friend bool operator==(const Foo &, const Foo &);
  friend bool operator==(Foo, Foo );
};


bool Foo::operator==(const Foo &) const = default;


bool operator==(const Foo &, const Foo &) = default;


bool operator==(Foo, Foo) = default;

namespace GH102588 {
struct A {
  int i = 0;
  constexpr operator int() const { return i; }
  constexpr operator int&() { return ++i; }
};

struct B : A {
  bool operator==(const B &) const = default;
};

constexpr bool f() {
  B x;
  return x == x;
}

static_assert(f());

struct ConstOnly {
  std::strong_ordering operator<=>(const ConstOnly&) const;
  std::strong_ordering operator<=>(ConstOnly&) = delete;
  friend bool operator==(const ConstOnly&, const ConstOnly&);
  friend bool operator==(ConstOnly&, ConstOnly&) = delete;
};

struct MutOnly {
  std::strong_ordering operator<=>(const MutOnly&) const = delete;;
  std::strong_ordering operator<=>(MutOnly&);
  friend bool operator==(const MutOnly&, const MutOnly&) = delete;;
  friend bool operator==(MutOnly&, MutOnly&);
};

struct ConstCheck : ConstOnly {
  friend std::strong_ordering operator<=>(const ConstCheck&, const ConstCheck&) = default;
  std::strong_ordering operator<=>(ConstCheck const& __restrict) const __restrict = default;
  friend bool operator==(const ConstCheck&, const ConstCheck&) = default;
  bool operator==(this const ConstCheck&, const ConstCheck&) = default;
};


struct MutCheck : MutOnly {
  friend bool operator==(MutCheck, MutCheck) = default;

  friend std::strong_ordering operator<=>(MutCheck, MutCheck) = default;

};
}
