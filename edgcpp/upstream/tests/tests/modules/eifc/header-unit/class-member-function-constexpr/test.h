class my_class {
  constexpr int return_one() { return 1; }
  static constexpr int return_two();
  static constexpr int return_three() { return 3; }
};

constexpr int my_class::return_two() {
  return 2;
}
