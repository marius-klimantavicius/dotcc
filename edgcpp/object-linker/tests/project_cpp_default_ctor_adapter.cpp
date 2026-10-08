// The array constructor callback omits source parameters with defaults, but
// evaluates the default expressions independently for each constructed object.
static int next_value;
static int next() { return ++next_value; }
struct Value {
  int value;
  int marker;
  Value(int n = next(), int m = 11) noexcept : value(n), marker(m) {}
};
int main() {
  Value array[3];
  if (next_value != 3) return 1;
  for (int i = 0; i != 3; ++i)
    if (array[i].value != i + 1 || array[i].marker != 11) return 2;
  Value direct(41, 43);
  if (next_value != 3 || direct.value != 41 || direct.marker != 43) return 3;
  Value second[2];
  if (next_value != 5 || second[0].value != 4 || second[1].value != 5) return 4;
  Value single;
  if (next_value != 6 || single.value != 6 || single.marker != 11) return 5;
  return 0;
}
