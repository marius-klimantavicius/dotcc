//type:rp
//options_all:--c++20

extern "C" int printf(const char*, ...);

int arr[3] = {1, 2, 3};

const volatile auto [cva, cvb, cvc] = arr;
const auto [ca, cb, cc] = arr;

void f() {
  static auto [sa, sb, sc] = arr;
  thread_local const auto [ta, tb, tc] = arr;

  printf("sa=%d, sb=%d, sc=%d\n", sa, sb, sc);
  sa += cva + ca + ta;
  sb += cvb + cb + tb;
  sc += cvc + cc + tc;
}

int main() {
  f();
  f();
}
