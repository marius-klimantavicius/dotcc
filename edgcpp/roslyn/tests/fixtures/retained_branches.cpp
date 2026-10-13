static_assert(sizeof(int) > 0, "retained static-assert message");

template<class T> requires (sizeof(T) > 0)
struct RequiredHeader { T value; };
RequiredHeader<int> required_header_instance;

struct FieldClosureOwner {
  int value = [] { return 17; }();
};

int ordinary_bound_context() {
  const int bound = 3;
  int values[bound]{};
  static_assert(sizeof(values) > 0, "retained local static-assert message");
  return sizeof(values);
}

template<class T> int dependent_bound_context() {
  const int bound = sizeof(T);
  int values[bound]{};
  return sizeof(values);
}

template<class T> int dependent_array_argument(T bound) {
  int values[bound];
  return sizeof(values);
}

int retained_branch_use() {
  FieldClosureOwner owner;
  return owner.value + ordinary_bound_context()
    + dependent_bound_context<long>() + dependent_array_argument(4);
}
