// Future owned source/physical ABI oracle: nontrivial copies, trivial cleanup.
// Original Value parameters/returns must remain source class values even when
// the lowered signature passes constructed objects and result buffers by pointer.
static int copies;
struct Value {
  int value;
  Value(int n) noexcept : value(n) {}
  Value(const Value& other) noexcept : value(other.value + 1) { ++copies; }
  Value successor(int amount) const noexcept { return Value(value + amount); }
};
Value make_value(int n) noexcept { return Value(n); }
int consume_value(Value value) noexcept { return value.value; }
struct Callbacks {
  Value (*make)(int) noexcept;
  int (*consume)(Value) noexcept;
  long (*unused)() noexcept;
};
static Callbacks callbacks = { make_value, consume_value, nullptr };
int main() {
  Value first = callbacks.make(7);
  if (first.value != 7 || copies != 0) return 1;
  // Physical member signature has TWO Value pointers, distinguished solely by
  // their owned result-buffer/receiver roles and actual EDG ordering.
  Value second = first.successor(4);
  if (first.value != 7 || second.value != 11 || copies != 0) return 2;
  if (callbacks.consume(first) != 8 || first.value != 7 || copies != 1) return 3;
  if (consume_value(second) != 12 || second.value != 11 || copies != 2) return 4;
  return 0;
}
