//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }

int main() {
  int x = 42;
  auto lambda = [x](int x) {}; // { dg-error "previously declared as a capture" }
}
