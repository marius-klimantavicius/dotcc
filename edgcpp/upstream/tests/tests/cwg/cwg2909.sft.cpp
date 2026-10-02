//options_all:--c++20 -A
  struct S {};
  int main() {
    constexpr S s;       // OK
    constexpr S s2 = s;  // OK
  }

//cwg: 2909
//title: Subtle difference between constant-initialized and constexpr
//meeting: Wroclaw 11/24
//edg_status: Passes
