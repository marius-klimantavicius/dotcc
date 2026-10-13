struct ExecutionBase { virtual ~ExecutionBase() {} };
struct ExecutionDerived : ExecutionBase { int value; };
int execution_cpp_casts(double value, ExecutionBase *base, int const *input) {
  int converted = static_cast<int>(value);
  auto derived = dynamic_cast<ExecutionDerived *>(base);
  auto mutable_pointer = const_cast<int *>(input);
  return converted + (derived != nullptr) + *mutable_pointer;
}
template<class T> concept ExecutionNumber = requires(T value) { value + 1; };
template<ExecutionNumber T> int execution_constraint(T value) { return value + 1; }
template<class T> int execution_constexpr(T value) {
  if constexpr(sizeof(T)==sizeof(int)) return value + 1;
  else return value + 2;
}
template<class T> int execution_dependent_switch(T value) {
  switch(value) { case sizeof(T): return 1; default:return 0; }
}
struct ExecutionValue {
  int value;
  ExecutionValue(int input):value(input) {}
  ExecutionValue(ExecutionValue const &other):value(other.value) {}
  ~ExecutionValue() {}
};
ExecutionValue execution_statement_result(int value) {
  return ({ ExecutionValue local(value); local; });
}
int execution_cpp_use(int value) {
  return execution_constraint(value) + execution_constexpr(value) + execution_dependent_switch(value);
}
