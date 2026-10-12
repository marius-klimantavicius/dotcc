template<class T> int selected_branch(T value) {
  if constexpr (sizeof(T) == sizeof(int)) return value + 1;
  else return 7;
}
struct SourceValue {
  int value;
  SourceValue(int v) : value(v) {}
  SourceValue(SourceValue const &other) : value(other.value) {}
  ~SourceValue() {}
  int read() const { return value; }
  static int static_read(int v) { return v + 2; }
};
SourceValue copy_result(SourceValue const &value) { return value; }
int ranges(int const (&values)[3]) {
  int sum = 0;
  for (int bias = 1; int value : values) sum += value + bias;
  return sum;
}
int indirect_jump(int choice) {
  void *destination = choice ? &&first : &&second;
  goto *destination;
first:
  return 3;
second:
  return 5;
}
int statement_result(int value) {
  return ({ int temporary = value + 2; temporary; });
}
int inline_assembly(int value) {
  int output;
  asm volatile ("mov %1, %0" : "=r" (output) : "r" (value) : "memory");
  asm goto ("" : : "r" (value) : "memory" : reached);
  output += 1;
reached:
  return output;
}
int source_statements(int value) {
  SourceValue object(value);
  int values[3] = {1, 2, 3};
  return ranges(values) + selected_branch(value) + indirect_jump(value)
    + statement_result(value) + object.read() + object.static_read(value);
}
