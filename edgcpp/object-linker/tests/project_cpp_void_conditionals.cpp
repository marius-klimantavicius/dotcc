static int trace;
static void append(int value) { trace = trace * 10 + value; }
static int consume(int value) { return value; }
static int select(bool condition) {
  condition ? append(1) : append(8);
  int snapshot = consume((condition ? append(2) : append(9), trace));
  if (snapshot != (condition ? 12 : 89)) return 1;
  return (condition ? (trace == 12 ? append(3) : append(7)) : append(6), 0);
}
static void maybe(int* value) { value ? append(*value) : (void)0; }
int main() {
  if (select(true) || trace != 123) return 1;
  trace = 0;
  if (select(false) || trace != 896) return 2;
  trace = 0;
  int value = 4;
  maybe(&value);
  maybe(nullptr);
  if (trace != 4) return 3;
  trace = 0;
  for (int i = 0; (i < 2 ? append(i + 1) : (void)0, i < 2); ++i) {}
  return trace != 12;
}
