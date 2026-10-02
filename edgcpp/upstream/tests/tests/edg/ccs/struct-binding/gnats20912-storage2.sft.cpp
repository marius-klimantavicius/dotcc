//type:fn
//options_all:--c++20

int arr[3] = {1, 2, 3};

extern const volatile auto [ea, eb, ec] = arr;
inline const auto [ia, ib, ic] = arr;

void f() {
  static auto [sa, sb, sc] = arr;
  thread_local const auto [ta, tb, tc] = arr;

  sa = ea + ia + ta;
  sb = eb + ib + tb;
  sc = ec + ic + tc;
}
