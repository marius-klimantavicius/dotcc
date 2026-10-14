struct DynamicEmptyBase { DynamicEmptyBase(int); };
struct DynamicEmptyField { DynamicEmptyField(int); };
struct DynamicAggregate : DynamicEmptyBase {
  [[no_unique_address]] DynamicEmptyField field;
  int value;
};
template<class T> void retained_dynamic_default(
    DynamicAggregate value = {DynamicEmptyBase(5), DynamicEmptyField(5), 5});

template<int N> void retained_dependent_static_bound() {
  static const int local_extent = 2;
  int values[N + local_extent];
  (void)values;
}
void use_retained_dependent_static_bound() {
  retained_dependent_static_bound<3>();
}
