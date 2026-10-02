//type: fn
//options:  --c++20 --exceptions
//filter:sed -E -e 's/line 210: note: the final comparison was -?[0-9]{4,} ==/line 210: note: the final comparison was VALUE ==/'
# 1 "SemaCXX/constant-expression-cxx2a.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/constant-expression-cxx2a.cpp" 2


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
# 4 "SemaCXX/constant-expression-cxx2a.cpp" 2

namespace std {
  struct type_info;
  struct destroying_delete_t {
    explicit destroying_delete_t() = default;
  } inline constexpr destroying_delete{};
  struct nothrow_t {
    explicit nothrow_t() = default;
  } inline constexpr nothrow{};
  using size_t = decltype(sizeof(0));
  enum class align_val_t : size_t {};
};

[[nodiscard]] void *operator new(std::size_t, const std::nothrow_t&) noexcept;
[[nodiscard]] void *operator new(std::size_t, std::align_val_t, const std::nothrow_t&) noexcept;
[[nodiscard]] void *operator new[](std::size_t, const std::nothrow_t&) noexcept;
[[nodiscard]] void *operator new[](std::size_t, std::align_val_t, const std::nothrow_t&) noexcept;
[[nodiscard]] void *operator new[](std::size_t, std::align_val_t);
void operator delete(void*, const std::nothrow_t&) noexcept;
void operator delete(void*, std::align_val_t, const std::nothrow_t&) noexcept;
void operator delete[](void*, const std::nothrow_t&) noexcept;
void operator delete[](void*, std::align_val_t, const std::nothrow_t&) noexcept;


constexpr void not_defined();
template<typename T> constexpr void print(T) { not_defined(); }

namespace ThreeWayComparison {
  struct A {
    int n;
    constexpr friend int operator<=>(const A &a, const A &b) {
      return a.n < b.n ? -1 : a.n > b.n ? 1 : 0;
    }
  };
  static_assert(A{1} <=> A{2} < 0);
  static_assert(A{2} <=> A{1} > 0);
  static_assert(A{2} <=> A{2} == 0);

  static_assert(1 <=> 2 < 0);
  static_assert(2 <=> 1 > 0);
  static_assert(1 <=> 1 == 0);
  constexpr int k = (1 <=> 1, 0);


  static_assert(std::strong_ordering::equal == 0);

  constexpr void f() {
    void(1 <=> 1);
  }

  struct MemPtr {
    void foo() {}
    void bar() {}
    int data;
    int data2;
    long data3;
  };

  struct MemPtr2 {
    void foo() {}
    void bar() {}
    int data;
    int data2;
    long data3;
  };
  using MemPtrT = void (MemPtr::*)();

  using FnPtrT = void (*)();

  void FnPtr1() {}
  void FnPtr2() {}




constexpr bool test_constexpr_success = [] {
  {
    auto &EQ = std::strong_ordering::equal;
    auto &LESS = std::strong_ordering::less;
    auto &GREATER = std::strong_ordering::greater;
    using SO = std::strong_ordering;
    auto eq = (42 <=> 42);
    static_assert(__is_same(decltype(eq), SO));;
    ((eq.test_eq(EQ)) ? void() : throw "error");

    auto less = (-1 <=> 0);
    static_assert(__is_same(decltype(less), SO));;
    ((less.test_eq(LESS)) ? void() : throw "error");

    auto greater = (42l <=> 1u);
    static_assert(__is_same(decltype(greater), SO));;
    ((greater.test_eq(GREATER)) ? void() : throw "error");
  }
  {
    using PO = std::partial_ordering;
    auto EQUIV = PO::equivalent;
    auto LESS = PO::less;
    auto GREATER = PO::greater;

    auto eq = (42.0 <=> 42.0);
    static_assert(__is_same(decltype(eq), PO));;
    ((eq.test_eq(EQUIV)) ? void() : throw "error");

    auto less = (39.0 <=> 42.0);
    static_assert(__is_same(decltype(less), PO));;
    ((less.test_eq(LESS)) ? void() : throw "error");

    auto greater = (-10.123 <=> -101.1);
    static_assert(__is_same(decltype(greater), PO));;
    ((greater.test_eq(GREATER)) ? void() : throw "error");
  }

  return true;
}();

int dummy = 42;
int dummy2 = 101;
constexpr bool tc9 = (&dummy <=> &dummy2) != 0;

template <class T, class R, class I>
constexpr T makeComplex(R r, I i) {
  T res{r, i};
  return res;
};
}

constexpr bool for_range_init() {
  int k = 0;
  for (int arr[3] = {1, 2, 3}; int n : arr) k += n;
  return k == 6;
}
static_assert(for_range_init());

namespace Virtual {
  struct NonZeroOffset { int padding = 123; };

  constexpr void assert(bool b) { if (!b) throw 0; }


  struct A {
    virtual constexpr char f() const { return 'A'; }
    char a = f();
    constexpr ~A() { assert(f() == 'A'); }
  };
  struct NoOverrideA : A {};
  struct B : NonZeroOffset, NoOverrideA {
    virtual constexpr char f() const { return 'B'; }
    char b = f();
    constexpr ~B() { assert(f() == 'B'); }
  };
  struct NoOverrideB : B {};
  struct C : NonZeroOffset, A {
    virtual constexpr char f() const { return 'C'; }
    A *pba;
    char c = ((A*)this)->f();
    char ba = pba->f();
    constexpr C(A *pba) : pba(pba) {}
    constexpr ~C() { assert(f() == 'C'); }
  };
  struct D : NonZeroOffset, NoOverrideB, C {
    virtual constexpr char f() const { return 'D'; }
    char d = f();
    constexpr D() : C((B*)this) {}
    constexpr ~D() { assert(f() == 'D'); }
  };
  constexpr int n = (D(), 0);
  constexpr D d;
  static_assert(((B&)d).a == 'A');
  static_assert(((C&)d).a == 'A');
  static_assert(d.b == 'B');
  static_assert(d.c == 'C');

  static_assert(d.ba == 'B');
  static_assert(d.d == 'D');
  static_assert(d.f() == 'D');
  constexpr const A &a = (B&)d;
  constexpr const B &b = d;
  static_assert(a.f() == 'D');
  static_assert(b.f() == 'D');


  D d_not_constexpr;
  static_assert(d_not_constexpr.f() == 'D');


  struct Covariant1 {
    D d;
    virtual const A *f() const;
  };
  template<typename T>
  struct Covariant2 : Covariant1 {
    virtual const T *f() const;
  };
  template<typename T>
  struct Covariant3 : Covariant2<T> {
    constexpr virtual const D *f() const { return &this->d; }
  };

  constexpr Covariant3<B> cb;
  constexpr Covariant3<C> cc;

  constexpr const Covariant1 *cb1 = &cb;
  constexpr const Covariant2<B> *cb2 = &cb;
  static_assert(cb1->f()->a == 'A');
  static_assert(cb1->f() == (B*)&cb.d);
  static_assert(cb1->f()->f() == 'D');
  static_assert(cb2->f()->b == 'B');
  static_assert(cb2->f() == &cb.d);
  static_assert(cb2->f()->f() == 'D');

  constexpr const Covariant1 *cc1 = &cc;
  constexpr const Covariant2<C> *cc2 = &cc;
  static_assert(cc1->f()->a == 'A');
  static_assert(cc1->f() == (C*)&cc.d);
  static_assert(cc1->f()->f() == 'D');
  static_assert(cc2->f()->c == 'C');
  static_assert(cc2->f() == &cc.d);
  static_assert(cc2->f()->f() == 'D');

  static_assert(cb.f()->d == 'D');
  static_assert(cc.f()->d == 'D');

  struct Abstract {
    constexpr virtual void f() = 0;
    constexpr Abstract() { do_it(); }
    constexpr void do_it() { f(); }
  };
  struct PureVirtualCall : Abstract { void f(); };
  constexpr PureVirtualCall pure_virtual_call;
}

namespace DynamicCast {
  struct A2 { virtual void a2(); };
  struct A : A2 { virtual void a(); };
  struct B : A {};
  struct C2 { virtual void c2(); };
  struct C : A, C2 { A *c = dynamic_cast<A*>(static_cast<C2*>(this)); };
  struct D { virtual void d(); };
  struct E { virtual void e(); };
  struct F : B, C, D, private E { void *f = dynamic_cast<void*>(static_cast<D*>(this)); };
  struct Padding { virtual void padding(); };
  struct G : Padding, F {};

  constexpr G g;


  static_assert(g.c == (C*)&g);

  static_assert(dynamic_cast<const A*>(static_cast<const C2*>(&g)) == nullptr);


  static_assert(g.f == (void*)(F*)&g);
  static_assert(dynamic_cast<const void*>(static_cast<const D*>(&g)) == &g);


  constexpr int d_a = (dynamic_cast<const A&>(static_cast<const D&>(g)), 0);


  static_assert(&dynamic_cast<A&>((A2&)(B&)g) == &(A&)(B&)g);

  static_assert(&dynamic_cast<A&>((B&)g) == &(A&)(B&)g);


  static_assert(&dynamic_cast<A&>((D&)g) == &(A&)(B&)g);


  static_assert(&dynamic_cast<D&>((A2&)(B&)g) == &(D&)g);



  constexpr int e_f = (dynamic_cast<F&>((E&)g), 0);



  constexpr int b_e = (dynamic_cast<E&>((B&)g), 0);

  struct Unrelated { virtual void unrelated(); };

  constexpr int b_unrelated = (dynamic_cast<Unrelated&>((B&)g), 0);

  constexpr int e_unrelated = (dynamic_cast<Unrelated&>((E&)g), 0);
}

namespace TypeId {
  struct A {
    const std::type_info &ti = typeid(*this);
  };
  struct A2 : A {};
  static_assert(&A().ti == &typeid(A));
  static_assert(&typeid((A2())) == &typeid(A2));
  extern A2 extern_a2;
  static_assert(&typeid(extern_a2) == &typeid(A2));

  constexpr A2 a2;
  constexpr const A &a1 = a2;
  static_assert(&typeid(a1) == &typeid(A));

  struct B {
    virtual void f();
    const std::type_info &ti1 = typeid(*this);
  };
  struct B2 : B {
    const std::type_info &ti2 = typeid(*this);
  };
  static_assert(&B2().ti1 == &typeid(B));
  static_assert(&B2().ti2 == &typeid(B2));
  extern B2 extern_b2;
  static_assert(&typeid(extern_b2) == &typeid(B2));

  constexpr B2 b2;
  constexpr const B &b1 = b2;
  static_assert(&typeid(b1) == &typeid(B2));

  constexpr bool side_effects() {

    bool OK = true;
    (void)typeid(OK = false, A2());
    if (!OK) return false;


    A2 a2;
    (void)typeid(OK = false, a2);
    if (!OK) return false;


    (void)typeid(OK = false, B2());
    if (!OK) return false;


    OK = false;
    B2 b2;
    (void)typeid(OK = true, b2);
    return OK;
  }
  static_assert(side_effects());
}

namespace Union {
  struct Base {
    int y;
  };
  struct A : Base {
    int x;
    int arr[3];
    union { int p, q; };
  };
  union B {
    A a;
    int b;
  };
  constexpr int read_wrong_member() {
    B b = {.b = 1};
    return b.a.x;
  }
  constexpr int change_member() {
    B b = {.b = 1};
    b.a.x = 1;
    return b.a.x;
  }
  static_assert(change_member() == 1);
  constexpr int change_member_then_read_wrong_member() {
    B b = {.b = 1};
    b.a.x = 1;
    return b.b;
  }
  constexpr int read_wrong_member_indirect() {
    B b = {.b = 1};
    int *p = &b.a.y;
    return *p;
  }
  constexpr int read_uninitialized() {
    B b = {.b = 1};
    int *p = &b.a.y;
    b.a.x = 1;
    return *p;
  }
  static_assert(read_uninitialized() == 0);
  constexpr void write_wrong_member_indirect() {
    B b = {.b = 1};
    int *p = &b.a.y;
    *p = 1;
  }
  constexpr int write_uninitialized() {
    B b = {.b = 1};
    int *p = &b.a.y;
    b.a.x = 1;
    *p = 1;
    return *p;
  }
  static_assert(write_uninitialized() == 1);
  constexpr int change_member_indirectly() {
    B b = {.b = 1};
    b.a.arr[1] = 1;
    int &r = b.a.y;
    r = 123;

    b.b = 2;
    b.a.y = 3;
    b.a.arr[2] = 4;
    return b.a.arr[2];
  }
  static_assert(change_member_indirectly() == 4);
  constexpr B return_uninit() {
    B b = {.b = 1};
    b.a.x = 2;
    return b;
  }
  constexpr B uninit = return_uninit();
  static_assert(return_uninit().a.x == 2);
  constexpr A return_uninit_struct() {
    B b = {.b = 1};
    b.a.x = 2;
    return b.a;
  }






  static_assert(return_uninit_struct().x == 2);
  constexpr B return_init_all() {
    B b = {.b = 1};
    b.a.x = 2;
    b.a.y = 3;
    b.a.arr[0] = 4;
    b.a.arr[1] = 5;
    b.a.arr[2] = 6;
    return b;
  }
  static_assert(return_init_all().a.x == 2);
  static_assert(return_init_all().a.y == 3);
  static_assert(return_init_all().a.arr[0] == 4);
  static_assert(return_init_all().a.arr[1] == 5);
  static_assert(return_init_all().a.arr[2] == 6);
  static_assert(return_init_all().a.p == 7);
  static_assert(return_init_all().a.q == 8);
  constexpr B init_all = return_init_all();

  constexpr bool test_no_member_change = []{
    union U { char dummy = {}; };
    U u1;
    U u2;
    u1 = u2;
    return true;
  }();

  struct S1 {
    int n;
  };
  struct S2 : S1 {};
  struct S3 : S2 {};
  void f() {
    S3 s;
    s.n = 0;
  }

  union ref_member_1 {
    int a;
    int b;
  };
  struct ref_member_2 {
    ref_member_1 &&r;
  };
  union ref_member_3 {
    ref_member_2 a, b;
  };
  constexpr int ref_member_test_1() {
    ref_member_3 r = {.a = {.r = {.a = 1}}};
    r.a.r.b = 2;
    return r.a.r.b;
  }
  static_assert(ref_member_test_1() == 2);
  constexpr int ref_member_test_2() {
    ref_member_3 r = {.a = {.r = {.a = 1}}};

    r.b.r.b = 2;
    return r.b.r.b;
  }

  namespace PR43762 {
    struct A { int x = 1; constexpr int f() { return 1; } };
    struct B : A { int y = 1; constexpr int g() { return 2; } };
    struct C {
      int x;
      constexpr virtual int f() = 0;
    };
    struct D : C {
      int y;
      constexpr virtual int f() override { return 3; }
    };

    union U {
      int n;
      B b;
      D d;
    };

    constexpr int test(int which) {
      U u{.n = 5};
      switch (which) {
      case 0:
        u.b.x = 10;
        return u.b.f();
      case 1:
        u.b.y = 10;
        return u.b.g();
      case 2:
        u.d.x = 10;
        return u.d.f();
      case 3:
        u.d.y = 10;
        return u.d.f();
      }
    }

    static_assert(test(0));
    static_assert(test(1));
    static_assert(test(2));
    static_assert(test(3));
  }
}

namespace TwosComplementShifts {
  using uint32 = unsigned int;
  using int32 = int;
  static_assert(uint32(int32(0x1234) << 16) == 0x12340000);
  static_assert(uint32(int32(0x1234) << 19) == 0x91a00000);
  static_assert(uint32(int32(0x1234) << 20) == 0x23400000);
  static_assert(uint32(int32(0x1234) << 24) == 0x34000000);
  static_assert(uint32(int32(-1) << 31) == 0x80000000);

  static_assert(-1 >> 1 == -1);
  static_assert(-1 >> 31 == -1);
  static_assert(-2 >> 1 == -1);
  static_assert(-3 >> 1 == -2);
  static_assert(-4 >> 1 == -2);
}

namespace Uninit {
  constexpr int f(bool init) {
    int a;
    if (init)
      a = 1;
    return a;
  }
  static_assert(f(true) == 1);
  static_assert(f(false) == 1);

  struct X {
    int n;
    constexpr X(bool init) {
      if (init) n = 123;
    }
  };
  constinit X x1(true);
  constinit X x2(false);

  struct Y {
    struct Z { int n; };
    Z z1;
    Z z2;
    Z z3;


    constexpr Y() : z2{ (z1.n = 1, z1.n + 1) } { z3.n = 3; }

    constexpr Y(int) : z2{
      (z3.n = 1,
       z3.n + 1)
    } { z1.n = 3; }
    constexpr Y(int, int) : z2{} {}
  };


  constexpr Y copy(Y y) { return y; }
  constexpr Y y1 = copy(Y());
  static_assert(y1.z1.n == 1 && y1.z2.n == 2 && y1.z3.n == 3);

  constexpr Y y2 = copy(Y(0));

  static_assert(Y(0,0).z2.n == 0);
  static_assert(Y(0,0).z1.n == 0);
  static_assert(Y(0,0).z3.n == 0);

  static_assert(copy(Y(0,0)).z2.n == 0);

  constexpr unsigned char not_even_unsigned_char() {
    unsigned char c;
    return c;
  }
  constexpr unsigned char x = not_even_unsigned_char();

  constexpr int switch_var(int n) {
    switch (n) {
    case 1:
      int a;
      a = n;
      return a;

    case 2:
      a = n;
      return a;
    }
  }
  constexpr int s1 = switch_var(1);
  constexpr int s2 = switch_var(2);
  static_assert(s1 == 1 && s2 == 2);

  constexpr bool switch_into_init_stmt() {
    switch (1) {
      if (int n; false) {
        for (int m; false;) {
        case 1:
          n = m = 1;
          return n == 1 && m == 1;
        }
      }
    }
  }
  static_assert(switch_into_init_stmt());
}

namespace dtor {
  void lifetime_extension() {
    struct X { constexpr ~X() {} };
    X &&a = X();
  }

  template<typename T> constexpr T &&ref(T &&t) { return (T&&)t; }

  struct Buf {
    char buf[64];
    int n = 0;
    constexpr void operator+=(char c) { buf[n++] = c; }
    constexpr bool operator==(const char *str) const {
      return str[n] == 0 && __builtin_memcmp(str, buf, n) == 0;
    }
    constexpr bool operator!=(const char *str) const { return !operator==(str); }
  };

  struct A {
    constexpr A(Buf &buf, char c) : buf(buf), c(c) { buf += c; }
    constexpr ~A() { buf += c; }
    constexpr operator bool() const { return true; }
    Buf &buf;
    char c;
  };

  constexpr bool dtor_calls_dtor() {
    union U {
      constexpr U(Buf &buf) : u(buf, 'u') { buf += 'U'; }
      constexpr ~U() { u.buf += 'U'; }
      A u, v;
    };

    struct B : A {
      A c, &&d, e;
      union {
        A f;
      };
      U u;
      constexpr B(Buf &buf)
          : A(buf, 'a'), c(buf, 'c'), d(ref(A(buf, 'd'))), e(A(buf, 'e')), f(buf, 'f'), u(buf) {
        buf += 'b';
      }
      constexpr ~B() {
        buf += 'b';
      }
    };

    Buf buf;
    {
      B b(buf);
      if (buf != "acddefuUb")
        return false;
    }
    if (buf != "acddefuUbbUeca")
      return false;
    return true;
  }
  static_assert(dtor_calls_dtor());

  constexpr void abnormal_termination(Buf &buf) {
    struct Indestructible {
      constexpr ~Indestructible();
    };

    A a(buf, 'a');
    A(buf, 'b');
    int n = 0;
    for (A &&c = A(buf, 'c'); A d = A(buf, 'd'); A(buf, 'e')) {
      switch (A f(buf, 'f'); A g = A(buf, 'g')) {
      case false: {
        A x(buf, 'x');
      }

      case true: {
        A h(buf, 'h');
        switch (n++) {
        case 0:
          break;
        case 1:
          continue;
        case 2:
          return;
        }
        break;
      }

      default:
        Indestructible indest;
      }

      A j = (A(buf, 'i'), A(buf, 'j'));
    }
  }

  constexpr bool check_abnormal_termination() {
    Buf buf = {};
    abnormal_termination(buf);
    return buf ==
      "abbc"
        "dfgh" "hgfijijeed"
        "dfgh" "hgfeed"
        "dfgh" "hgfd"
      "ca";
  }
  static_assert(check_abnormal_termination());

  constexpr bool run_dtors_on_array_filler() {
    struct S {
      int times_destroyed = 0;
      constexpr ~S() { if (++times_destroyed != 1) throw "oops"; }
    };
    S s[3];
    return true;
  }
  static_assert(run_dtors_on_array_filler());


  struct ArrElem { constexpr ~ArrElem() {} };
  using Arr = ArrElem[3];
  static_assert(((void)Arr{}, true));
}

namespace dynamic_alloc {
  constexpr int *p =
    new int;

  constexpr int f(int n) {
    int *p = new int[n];
    for (int i = 0; i != n; ++i) {
      p[i] = i;
    }
    int k = 0;
    for (int i = 0; i != n; ++i) {
      k += p[i];
    }
    delete[] p;
    return k;
  }
  static_assert(f(123) == 123 * 122 / 2);

  constexpr bool nvdtor() {
    struct S {
      constexpr ~S() {}
    };
    struct T : S {};
    delete (S*)new T;
    return true;
  }

  constexpr int vdtor_1() {
    int a;
    struct S {
      constexpr S(int *p) : p(p) {}
      constexpr virtual ~S() { *p = 1; }
      int *p;
    };
    struct T : S {

      using S::S;
    };
    delete (S*)new T(&a);
    return a;
  }
  static_assert(vdtor_1() == 1);

  constexpr int vdtor_2() {
    int a = 0;
    struct S { constexpr virtual ~S() {} };
    struct T : S {
      constexpr T(int *p) : p(p) {}
      constexpr ~T() { ++*p; }
      int *p;
    };
    S *p = new T{&a};
    delete p;
    return a;
  }
  static_assert(vdtor_2() == 1);

  constexpr int vdtor_3(int mode) {
    int a = 0;
    struct S { constexpr virtual ~S() {} };
    struct T : S {
      constexpr T(int *p) : p(p) {}
      constexpr ~T() { ++*p; }
      int *p;
    };
    S *p = new T[3]{&a, &a, &a};
    switch (mode) {
    case 0:
      delete p;
      break;
    case 1:


      delete[] p;
      break;
    case 2:
      delete (T*)p;
      break;
    case 3:
      delete[] (T*)p;
      break;
    }
    return a;
  }
  static_assert(vdtor_3(0) == 3);
  static_assert(vdtor_3(1) == 3);
  static_assert(vdtor_3(2) == 3);
  static_assert(vdtor_3(3) == 3);

  constexpr void delete_mismatch() {
    delete[]
      new int;
  }

  template<typename T>
  constexpr T dynarray(int elems, int i) {
    T *p;
    if constexpr (sizeof(T) == 1)
      p = new T[elems]{"fox"};
    else
      p = new T[elems]{1, 2, 3};
    T n = p[i];
    delete [] p;
    return n;
  }
  static_assert(dynarray<int>(4, 0) == 1);
  static_assert(dynarray<int>(4, 1) == 2);
  static_assert(dynarray<int>(4, 2) == 3);
  static_assert(dynarray<int>(4, 3) == 0);
  static_assert(dynarray<int>(4, 4) == 0);
  static_assert(dynarray<int>(3, 2) == 3);
  static_assert(dynarray<int>(3, 3) == 0);
  static_assert(dynarray<int>(2, 1) == 0);
  static_assert(dynarray<char>(5, 0) == 'f');
  static_assert(dynarray<char>(5, 1) == 'o');
  static_assert(dynarray<char>(5, 2) == 'x');
  static_assert(dynarray<char>(5, 3) == 0);
  static_assert(dynarray<char>(5, 4) == 0);
  static_assert(dynarray<char>(5, 5) == 0);
  static_assert(dynarray<char>(4, 0) == 'f');
  static_assert(dynarray<char>(4, 1) == 'o');
  static_assert(dynarray<char>(4, 2) == 'x');
  static_assert(dynarray<char>(4, 3) == 0);
  static_assert(dynarray<char>(4, 4) == 0);
  static_assert(dynarray<char>(3, 2) == 'x');

  constexpr bool run_dtors_on_array_filler() {
    struct S {
      int times_destroyed = 0;
      constexpr ~S() { if (++times_destroyed != 1) throw "oops"; }
    };
    delete[] new S[3];
    return true;
  }
  static_assert(run_dtors_on_array_filler());

  constexpr bool erroneous_array_bound(long long n) {
    delete[] new int[n];
    return true;
  }
  static_assert(erroneous_array_bound(3));
  static_assert(erroneous_array_bound(0));
  static_assert(erroneous_array_bound(-1));
  static_assert(erroneous_array_bound(1LL << 62));

  constexpr bool erroneous_array_bound_nothrow(long long n) {
    int *p = new (std::nothrow) int[n];
    bool result = p != 0;
    delete[] p;
    return result;
  }
  static_assert(erroneous_array_bound_nothrow(3));
  static_assert(erroneous_array_bound_nothrow(0));
  static_assert(!erroneous_array_bound_nothrow(-1));
  static_assert(!erroneous_array_bound_nothrow(1LL << 62));

  constexpr bool evaluate_nothrow_arg() {
    bool ok = false;
    delete new ((ok = true, std::nothrow)) int;
    return ok;
  }
  static_assert(evaluate_nothrow_arg());

  constexpr void double_delete() {
    int *p = new int;
    delete p;
    delete p;
  }
  constexpr bool super_secret_double_delete() {
    struct A {
      constexpr ~A() { delete this; }
    };
    delete new A;
    return true;
  }
  static_assert(super_secret_double_delete());

  constexpr void use_after_free() {
    int *p = new int;
    delete p;
    *p = 1;
  }
  constexpr void use_after_free_2() {
    struct X { constexpr void f() {} };
    X *p = new X;
    delete p;
    p->f();
  }

  template<typename T> struct X {
    std::size_t n;
    char *p;
    void dependent();
  };
  template<typename T> void X<T>::dependent() {
    char *p;


    p = new char[n];
    p = new ((std::align_val_t)n) char[n];
    p = new char(n);
  }

  namespace PR47143 {
    constexpr char *f(int n) {
      return new char[n]();
    }
    const char *p = f(3);
    constexpr bool test() {
      char *p = f(3);
      bool result = !p[0] && !p[1] && !p[2];
      delete [] p;
      return result;
    }
    static_assert(test());
  }
}

struct placement_new_arg {};
void *operator new(std::size_t, placement_new_arg);
void operator delete(void*, placement_new_arg);

namespace placement_new_delete {
  struct ClassSpecificNew {
    void *operator new(std::size_t);
  };
  struct ClassSpecificDelete {
    void operator delete(void*);
  };
  struct DestroyingDelete {
    void operator delete(DestroyingDelete*, std::destroying_delete_t);
  };
  struct alignas(64) Overaligned {};

  constexpr bool ok() {
    delete new Overaligned;
    delete ::new ClassSpecificNew;
    ::delete new ClassSpecificDelete;
    ::delete new DestroyingDelete;
    return true;
  }
  static_assert(ok());

  constexpr bool bad(int which) {
    switch (which) {
    case 0:
      delete new (placement_new_arg{}) int;
      break;

    case 1:
      delete new ClassSpecificNew;
      break;

    case 2:
      delete new ClassSpecificDelete;
      break;

    case 3:
      delete new DestroyingDelete;
      break;

    case 4:


      delete new (std::align_val_t{64}) Overaligned;
      break;
    }

    return true;
  }
  static_assert(bad(0));
  static_assert(bad(1));
  static_assert(bad(2));
  static_assert(bad(3));
  static_assert(bad(4));
}

namespace delete_random_things {
  static_assert((delete new int, true));
  static_assert((delete (int*)0, true));
  int n;
  static_assert((delete &n, true));
  struct A { int n; };
  static_assert((delete &(new A)->n, true));
  static_assert((delete (new int + 1), true));
  static_assert((delete[] (new int[3] + 1), true));
  static_assert((delete &(int&)(int&&)0, true));
}

namespace value_dependent_delete {
  template<typename T> void f(T *p) {
    int arr[(delete p, 0)];
  }
}

namespace memory_leaks {
  static_assert(*new bool(true));

  constexpr bool *f() { return new bool(true); }
  static_assert(*f());

  struct UP {
    bool *p;
    constexpr ~UP() { delete p; }
    constexpr bool &operator*() { return *p; }
  };
  constexpr UP g() { return {new bool(true)}; }
  static_assert(*g());

  constexpr bool h(UP p) { return *p; }
  static_assert(h({new bool(true)}));
}

constexpr void *operator new(std::size_t, void *p) { return p; }
namespace std {
  template<typename T> constexpr T *construct(T *p) { return new (p) T; }
  template<typename T> constexpr void destroy(T *p) { p->~T(); }
}

namespace dtor_call {
  struct A { int n; };
  constexpr void f() {
    A a;
    a.~A();
  }
  union U { A a; };
  constexpr void g() {
    U u;
    u.a.n = 3;
    u.a.~A();


    u.a.n = 4;
    u.a.~A();
  }
  static_assert((g(), true));

  constexpr bool pseudo(bool read, bool recreate) {
    using T = bool;
    bool b = false;

    (b = true).~T();

    return (read
            ? b
            : true) +
           (recreate
            ? (std::construct(&b), true)
            : true);
  }
  static_assert(pseudo(false, false));
  static_assert(pseudo(true, false));
  static_assert(pseudo(false, true));

  constexpr void use_after_destroy() {
    A a;
    a.~A();
    A b = a;
  }
  static_assert((use_after_destroy(), true));

  constexpr void double_destroy() {
    A a;
    a.~A();
    a.~A();
  }
  static_assert((double_destroy(), true));

  struct X { char *p; constexpr ~X() { *p++ = 'X'; } };
  struct Y : X { int y; virtual constexpr ~Y() { *p++ = 'Y'; } };
  struct Z : Y { int z; constexpr ~Z() override { *p++ = 'Z'; } };
  union VU {
    constexpr VU() : z() {}
    constexpr ~VU() {}
    Z z;
  };

  constexpr bool virt_dtor(int mode, const char *expected) {
    char buff[4] = {};
    VU vu;
    vu.z.p = buff;
    switch (mode) {
    case 0:
      vu.z.~Z();
      break;
    case 1:
      ((Y&)vu.z).~Y();
      break;
    case 2:
      ((X&)vu.z).~X();
      break;
    case 3:
      ((Y&)vu.z).Y::~Y();
      vu.z.z = 1;
      break;
    case 4:
      ((X&)vu.z).X::~X();
      vu.z.y = 1;
      break;
    }
    return __builtin_strcmp(expected, buff) == 0;
  }
  static_assert(virt_dtor(0, "ZYX"));
  static_assert(virt_dtor(1, "ZYX"));
  static_assert(virt_dtor(2, "X"));
  static_assert(virt_dtor(3, "YX"));
  static_assert(virt_dtor(4, "X"));

  constexpr bool virt_delete(bool global) {
    struct A {
      virtual constexpr ~A() {}
    };
    struct B : A {
      void operator delete(void *);
      constexpr ~B() {}
    };

    A *p = new B;
    if (global)
      ::delete p;
    else
      delete p;
    return true;
  }
  static_assert(virt_delete(true));
  static_assert(virt_delete(false));

  constexpr void use_after_virt_destroy() {
    char buff[4] = {};
    VU vu;
    vu.z.p = buff;
    ((Y&)vu.z).~Y();
    ((Z&)vu.z).z = 1;
  }
  static_assert((use_after_virt_destroy(), true));

  constexpr void destroy_after_lifetime() {
    A *p;
    {
      A a;
      p = &a;
    }
    p->~A();
  }
  static_assert((destroy_after_lifetime(), true));

  constexpr void destroy_after_lifetime2() {
    A *p = []{ A a; return &a; }();
    p->~A();
  }
  static_assert((destroy_after_lifetime2(), true));

  constexpr void destroy_after_lifetime3() {
    A *p = []{ return &(A&)(A&&)A(); }();
    p->~A();
  }
  static_assert((destroy_after_lifetime3(), true));

  constexpr void destroy_after_lifetime4() {
    A *p = new A;
    delete p;
    p->~A();
  }

  struct Extern { constexpr ~Extern() {} } extern e;
  constexpr void destroy_extern() {
    e.~Extern();
  }

  constexpr A &&a_ref = A();
  constexpr void destroy_extern_2() {
    a_ref.~A();
  }

  struct S {
    constexpr S() { n = 1; }
    constexpr ~S() { n = 0; }
    int n;
  };
  constexpr void destroy_volatile() {
    volatile S s;
  }
  static_assert((destroy_volatile(), true));

  constexpr void destroy_null() {
    ((A*)nullptr)->~A();
  }

  constexpr void destroy_past_end() {
    A a;
    (&a+1)->~A();
  }

  constexpr void destroy_past_end_array() {
    A a[2];
    a[2].~A();
  }

  union As {
    A a, b;
  };

  constexpr void destroy_no_active() {
    As as;
    as.b.~A();
  }

  constexpr void destroy_inactive() {
    As as;
    as.a.n = 1;
    as.b.~A();
  }

  constexpr void destroy_no_active_2() {
    As as;
    as.a.n = 1;
    as.a.~A();

    as.b.~A();
  }

  constexpr void destroy_pointer() {
    using T = int*;
    T p;


    p.~T();

    std::construct(&p);
  }
  static_assert((destroy_pointer(), true));
}

namespace temp_dtor {
  void f();
  struct A {
    bool b;
    constexpr ~A() { if (b) f(); }
  };




  constexpr A &&a = A{false};
  void f() { a.b = true; }

  constexpr A &&b = A{true};


  constexpr const A &c = A{false};
}

namespace value_dependent_init {
  struct A {
    constexpr ~A() {}
  };
  template<typename T> void f() {
    A a = T();
  }
}

namespace mutable_subobjects {
  struct A {
    int m;
    mutable int n;
    constexpr int f() const { return m; }
    constexpr int g() const { return n; }
  };

  constexpr A a = {1, 2};
  static_assert(a.f() == 1);
  static_assert(a.g() == 2);

  constexpr A b = a;

  auto &ti1 = typeid(a);
  auto &ti2 = typeid(a.m);
  auto &ti3 = typeid(a.n);

  constexpr void destroy1() {
    a.~A();
  }
  using T = int;
  constexpr void destroy2() {
    a.m.~T();
  }
  constexpr void destroy3() {
    a.n.~T();
  }

  struct X {
    mutable int n = 0;
    virtual constexpr ~X() {}
  };
  struct Y : X {
  };
  constexpr Y y;
  constexpr const X *p = &y;
  constexpr const Y *q = dynamic_cast<const Y*>(p);





  struct Z {
    mutable Y y;
  };
  constexpr Z z;
  constexpr const X *pz = &z.y;
  constexpr const Y *qz = dynamic_cast<const Y*>(pz);
  auto &zti = typeid(z.y);
  static_assert(&zti == &typeid(Y));
}

namespace PR45133 {
  struct A { long x; };

  union U;
  constexpr A foo(U *up);

  union U {
    A a = foo(this);
    int y;
  };

  constexpr A foo(U *up) {
    up->y = 11;
    return {42};
  }

  constinit U u = {};

  template<int> struct X {};

  union V {
    int a, b;
    constexpr V(X<0>) : a(a = 1) {}
    constexpr V(X<1>) : a(b = 1) {}
    constexpr V(X<2>) : a() { b = 1; }


    constexpr V(X<3>) : a((b = 1, a = 1)) {}
  };
  constinit V v0 = X<0>();
  constinit V v1 = X<1>();
  constinit V v2 = X<2>();
  constinit V v3 = X<3>();
}

namespace PR45350 {
  int q;
  struct V { int n; int *p = &n; constexpr ~V() { *p = *p * 10 + n; }};
  constexpr int f(int n) {
    int k = 0;
    V *p = new V[n];
    for (int i = 0; i != n; ++i) {
      if (p[i].p != &p[i].n) return -1;
      p[i].n = i;
      p[i].p = &k;
    }
    delete[] p;
    return k;
  }



  static_assert(f(6) == 543210);
}

namespace PR47805 {
  struct A {
    bool bad = true;
    constexpr ~A() { if (bad) throw; }
  };
  constexpr bool f(A a) { a.bad = false; return true; }
  constexpr bool b = f(A());

  struct B { B *p = this; };
  constexpr bool g(B b) { return &b == b.p; }
  static_assert(g({}));
}

constexpr bool destroy_at_test() {
  int n = 0;
  std::destroy(&n);
  std::construct(&n);
  return true;
}
static_assert(destroy_at_test());

namespace PR48582 {
  struct S {
    void *p = this;
    constexpr S() {}
    constexpr S(const S&) {}
  };
  constexpr bool b = [a = S(), b = S()] { return a.p == b.p; }();
  static_assert(!b);
}

namespace PR45879 {
  struct A { int n; };
  struct B { A a; };
  constexpr A a = (A() = B().a);

  union C {
    int n;
    A a;
  };

  constexpr bool f() {
    C c = {.n = 1};
    c.a = B{2}.a;
    return c.a.n == 2;
  }
  static_assert(f());


  constexpr bool g() {
    C c = {.n = 1};
    c.a.operator=(B{2}.a);
    return c.a.n == 2;
  }
  static_assert(g());
}

namespace GH57431 {
class B {
  virtual int constexpr f() = 0;
};

class D : B {
  virtual int constexpr f() = default;
};
}

namespace GH57516 {
class B{
  virtual constexpr ~B() = 0;
};

class D : B{};

}

namespace GH67317 {
  constexpr unsigned char a =

    __builtin_bit_cast(unsigned char, *new char[3][1]);
};

namespace GH150705 {
  struct A { };
  struct B : A { };
  struct C : A {
    constexpr virtual int foo() const { return 0; }
  };
  constexpr auto p = &C::foo;
  constexpr auto q = static_cast<int (A::*)() const>(p);
  constexpr B b;
  constexpr const A& a = b;
  constexpr auto x = (a.*q)();
}
