//remark:decltype of structured binding with dependent initializer
//options:--c++17;fp

struct S { int member; };
S f(int i) { return S{i}; }
template<typename T> bool v = T{};
template <typename T> int g(T p) {
  auto [b] = f(p);
  return v<decltype(b)>;
}
int r = g(42);

