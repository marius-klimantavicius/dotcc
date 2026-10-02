//remark:P2579 -- tweaks to captures
//options:--c++20;fp

void g() {
  int x;
  int const y = 0;
  [=]() -> decltype((x)) {
    return y;
  };
}
