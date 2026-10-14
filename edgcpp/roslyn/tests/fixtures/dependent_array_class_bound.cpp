template<int N> void retained_dependent_class_bound() {
  static const int local_extent = 2;
  struct BoundLocal {
    int values[N + local_extent];
  };
  BoundLocal object{};
  (void)object;
}
void use_retained_dependent_class_bound() {
  retained_dependent_class_bound<3>();
}
