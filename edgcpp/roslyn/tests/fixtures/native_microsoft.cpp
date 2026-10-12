struct NativeProperty {
  int value;
  int get() const { return value; }
  void set(int next) { value = next; }
  __declspec(property(get=get, put=set)) int property;
};
int native_for_each(int (&values)[3]) {
  int total = 0;
  for each (int value in values) total += value;
  int existing = 0;
  for each (existing in values) total += existing;
  return total;
}
int native_seh(int value) {
  __try { if (value) __leave; value += 2; }
  __finally { value += 3; }
  __try { value += 4; }
  __except (1) { value = 5; }
  return value;
}
int native_microsoft(int value) {
  NativeProperty object{value};
  object.property = value + 1;
  int values[3] = {1, 2, 3};
  return object.property + native_for_each(values) + native_seh(value);
}
