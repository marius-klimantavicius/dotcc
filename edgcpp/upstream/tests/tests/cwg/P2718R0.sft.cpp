//type:rp
//options_all:--c++23
bool alive = false;
struct A {
  A() { alive = true; }
  ~A() { alive = false; }
  // Set up dummy begin/end to force infinite loop:
  A *begin() const { return nullptr; }
  A *end() const { return (A*)1; }
};
using T = A;
const T& f1(const T& t) { return t; }
T g() { return A(); }
int main() {
  for (auto e : f1(g())) {
    if (!alive)
      return 1;  // Lifetime of return value of g() was not extended
    break;
  }
  return 0;
}
