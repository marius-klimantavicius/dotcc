class my_class {
  inline int return_one() { return 1; }
  static inline int return_two();
  static inline int return_three() { return 3; }
};

int my_class::return_two() {
  return 2;
}
