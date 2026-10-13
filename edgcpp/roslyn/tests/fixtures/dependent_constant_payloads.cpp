template<class T> int dependent_template_call(T& object) {
  return object.template apply<int, 3>(4);
}
struct ConcreteApply {
  template<class U, int N> int apply(U value) { return value + N; }
};
int use_apply(ConcreteApply& value) { return dependent_template_call(value); }

template<class T> void dependent_destroy(T* pointer) { pointer->~T(); }
struct Destroyable { ~Destroyable(); };
void use_destroy(Destroyable* pointer) { dependent_destroy(pointer); }

template<int... Values> struct IntegerValues {};
template<int N> struct IntegerPackHost {
  using type = IntegerValues<__integer_pack(N)...>;
};
IntegerPackHost<3>::type integer_values;

template<int N> int dependent_designator() {
  int values[8] = { [N] = 7 };
  return values[N];
}
template<int First, int Last> int dependent_range_designator() {
  int values[8] = { [First ... Last] = 9 };
  return values[First];
}

struct EmptyBase {};
struct EmptyField {};
struct EmptyAggregate : EmptyBase {
  [[no_unique_address]] EmptyField field;
  int value;
};
template<EmptyAggregate Value = EmptyAggregate{{}, {}, 17}>
struct KeepsEmptyDefault {};
KeepsEmptyDefault<> actual_default;

struct DynamicEmptyBase { DynamicEmptyBase(int); };
struct DynamicEmptyField { DynamicEmptyField(int); };
struct DynamicAggregate : DynamicEmptyBase {
  [[no_unique_address]] DynamicEmptyField field;
  int value;
};
template<class T> int retained_empty_init(int n) {
  DynamicAggregate value{DynamicEmptyBase(n), DynamicEmptyField(n), n};
  return value.value;
}
int actual_empty_init() { return retained_empty_init<int>(3); }

int use_designators() {
  return dependent_designator<2>() + dependent_range_designator<1, 3>();
}

template<class T> void retained_dynamic_default(
    DynamicAggregate value = {DynamicEmptyBase(5), DynamicEmptyField(5), 5});
