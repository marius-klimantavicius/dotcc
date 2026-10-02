//type:fn
//options_all:--c++20 -tused -A
consteval int id(int i) { return i; }
constexpr char id(char c) { return c; }

template <typename T>
constexpr int f(T t) {
    return t + id(t);
}

auto b = &f<int>;  // error: f<int> is an immediate function

