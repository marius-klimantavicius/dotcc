//remark:P2579 -- tweaks to captures
//options:--c++20;fn

void f() {
  int x = 0;
  auto g0 = [x=0](int x) { return 0; };  // error: duplicate name
  auto g1 = [x](int x) { return 0; };  // error: duplicate name
  auto h =
    [y = 0]<typename y> {  // error: duplicate name
      return 0; };
}

