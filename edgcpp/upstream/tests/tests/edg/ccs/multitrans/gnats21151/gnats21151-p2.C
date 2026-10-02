namespace a {
  template <bool, typename> using c = bool;
  template <typename, int> struct array { int d; };
  template <typename b> array(b)->array<c<0, b>, 1>;
}

void f() {
  (void)a::array{3};
}
