//options_all:--c++23 -A

  auto x = [](int i, auto a) { return i; };             // OK, a generic lambda
  auto y = [](this auto self, int i) { return i; };      // OK, a generic lambda
  auto z = []<class T>(int i) { return i; };             // OK, a generic lambda
