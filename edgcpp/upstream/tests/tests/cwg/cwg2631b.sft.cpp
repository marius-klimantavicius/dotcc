//type:fp
//options_all:--c++20 -tused 
constexpr int g();
consteval int f() {
  return g();
}

int k(int x = f()) {  // error: constexpr evaluation of undefined function g
  return x;
}

constexpr int g() {
  return 42;
}

int main() {
  return k();
}
