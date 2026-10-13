[uuid("34567890-1234-1234-1234-123456789abc"), no_injected_text]
struct AttributeBlock { int value; };

struct NativeFieldIterator {
  int current;
  int sum(int (&values)[3]) {
    int total = 0;
    for each (current in values) total += current;
    return total;
  }
};
int use_field_iterator(int (&values)[3]) {
  NativeFieldIterator object{};
  return object.sum(values);
}
int grouped_attribute_value() { return sizeof(AttributeBlock); }
