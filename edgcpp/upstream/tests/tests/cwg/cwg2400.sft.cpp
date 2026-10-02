//type:fn
//options_all:--c++20 -A -tused
struct A {
    constexpr virtual int f() const {
      return 1;
    }
  };

  struct B : A {
    constexpr virtual int f() const {
      return 2;
    }
  };

  constexpr B b{};
  constexpr A&& ref = (B)b;

  static_assert(ref.f() == 2, "");

//cwg: 2400
//title: Constexpr virtual functions and temporary objects
//meeting: Cologne 07/19
//edg_status: Passes
