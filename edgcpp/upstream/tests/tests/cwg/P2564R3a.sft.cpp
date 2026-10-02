//type:fp
//options_all:--c++20 -tused -A
consteval int id(int i) { return i; }
constexpr char id(char c) { return c; }

template <typename T>
constexpr int f(T t) {
    return t + id(t);
}

auto a = &f<char>; // ok, f<char> is not an immediate function

static_assert(f(3) == 6); // ok

template <typename T>
constexpr int g(T t) {    // g<int> is not an immediate function
    return t + id(42);    // because id(42) is already a constant
}

template <typename T, typename F>
constexpr bool is_not(T t, F f) {
    return not f(t);
}

consteval bool is_even(int i) { return i % 2 == 0; }

static_assert(is_not(5, is_even)); // ok

int x = 0;

template <typename T>
constexpr T h(T t = id(x)) { // h<int> is not an immediate function
    return t;
}

template <typename T>
constexpr T hh() {           // hh<int> is an immediate function
    return h<T>();
}


struct A {
  int x;
  int y = id(x);
};

template <typename T>
constexpr int k(int) {  // k<int> is not an immediate function
  return A(42).y;       // because A(42) is a constant expression and thus not
}                       // immediate-escalating
