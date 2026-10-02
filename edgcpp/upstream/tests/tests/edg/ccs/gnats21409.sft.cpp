//type:cp
//options_all:--c++17

template <class T> struct _AAA {
  T val;
  constexpr operator T() {return val;}
};

auto _le = [](auto val) constexpr {
	     typedef decltype(val) T;
	     _AAA<T> ret{val};
	     return ret;
	   };

struct A {
  int x;
  constexpr A(int i) : x(i) {}
  constexpr A(const A& obj) : x(obj.x + 10) {}
};

constexpr A a(37);
constexpr A b(_le(a));

static_assert(a.x == 37);
static_assert(_le(a).val.x == 67);
static_assert(b.x == 77);
