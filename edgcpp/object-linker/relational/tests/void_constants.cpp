constexpr void no_value() {}

int main() {
  static_cast<void>(0);
  (void)0;
  no_value();
  return 0;
}
