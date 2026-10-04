struct TokenRow {
  const char *text;
  unsigned short length;
  unsigned short token;
  unsigned short flags;
};
static TokenRow table[5] = {
  {"alpha", 5, 65535, 32768},
  {"prefix-tail" + 7, 4, 17, 9},
  {"zero tail"},
  {nullptr, 0, 1},
};
static TokenRow independent[2] = {{"alpha", 5, 65535, 32768}, {"prefix-tail" + 7, 4, 17, 9}};
enum Kind : unsigned short { small_kind = 3, large_kind = 65535 };
struct Plain { short minimum; unsigned maximum; };
struct Nested { const char *name; Plain point; Kind kind; unsigned long long wide; };
static Nested nested_rows[3] = {
  {"nested", {-32768, 0xffffffffu}, large_kind, 0xffffffffffffffffULL},
  {"partial", {7}, small_kind},
  {},
};
union Word { unsigned value; float floating; };
struct UnionRow { Word word; unsigned short tag; };
static UnionRow union_rows[2] = {{{0xf1234567u}, 65535}, {{17}, 8}};
struct BitRow { unsigned first : 3; signed second : 5; unsigned short tag; };
static BitRow bit_rows[2] = {{7, -16, 65535}, {3, 15, 17}};
struct ArrayRow { unsigned short values[3]; const char *label; };
static ArrayRow array_rows[2] = {{{1, 65535, 32768}, "array"}, {{7}, "partial"}};
static int globals[4] = {17, -23, 42, 99};
struct Relocation { int *target; const char *text; unsigned short index; };
static Relocation relocated[3] = {{&globals[2], "offset:text" + 7, 2}, {&globals[0], "first", 0}, {nullptr}};
struct Node { const char *name; Node *next; unsigned short tag; };
static Node nodes[3] = {{"head", &nodes[1], 1}, {"middle", &nodes[2], 2}, {"tail", &nodes[0], 3}};
static thread_local TokenRow tls_rows[3] = {{"thread", 6, 65535, 1}, {"prefix-tail" + 7, 4, 17, 9}};
// These source names collide with the TLS initializer's own storage temporaries.
static thread_local TokenRow p[1] = {{"thread p", 8, 2, 3}};
static thread_local TokenRow storage[1] = {{"thread storage", 14, 5, 7}};

static unsigned mix(unsigned a, unsigned b) { return (a ^ b) * 16777619u; }
static unsigned text_hash(const char *text) {
  unsigned value = 2166136261u;
  if (!text) return 0;
  while (*text) value = mix(value, static_cast<unsigned char>(*text++));
  return value;
}
static unsigned row_hash(const TokenRow *rows, unsigned count) {
  unsigned value = 2166136261u;
  for (unsigned i = 0; i != count; ++i) {
    value = mix(value, text_hash(rows[i].text));
    value = mix(value, rows[i].length);
    value = mix(value, rows[i].token);
    value = mix(value, rows[i].flags);
  }
  return value;
}
extern "C" unsigned ri_snapshot() {
  unsigned value = row_hash(table, 5) ^ row_hash(independent, 2);
  for (unsigned i = 0; i != 3; ++i) {
    value = mix(value, text_hash(nested_rows[i].name));
    value = mix(value, static_cast<unsigned>(nested_rows[i].point.minimum));
    value = mix(value, nested_rows[i].point.maximum);
    value = mix(value, nested_rows[i].kind);
    value = mix(value, static_cast<unsigned>(nested_rows[i].wide));
    value = mix(value, static_cast<unsigned>(nested_rows[i].wide >> 32));
  }
  return value;
}
extern "C" unsigned ri_fallback() {
  return union_rows[0].word.value == 0xf1234567u && union_rows[0].tag == 65535
      && union_rows[1].word.value == 17 && bit_rows[0].first == 7
      && bit_rows[0].second == -16 && bit_rows[0].tag == 65535
      && bit_rows[1].first == 3 && bit_rows[1].second == 15
      && array_rows[0].values[1] == 65535 && array_rows[0].values[2] == 32768
      && array_rows[1].values[0] == 7 && array_rows[1].values[1] == 0 && array_rows[1].values[2] == 0
      && relocated[0].target == &globals[2] && *relocated[0].target == 42
      && relocated[0].text[0] == 't' && relocated[0].text[3] == 't'
      && relocated[1].target == &globals[0] && relocated[2].target == nullptr
      && table[1].text[0] == 't' && table[1].text[3] == 'l'
      && nodes[0].next == &nodes[1] && nodes[0].next->next == &nodes[2]
      && nodes[0].next->next->next == &nodes[0] && nodes[1].next->tag == 3;
}
extern "C" void ri_mutate() {
  table[0].token = 19; table[4].text = "last"; table[4].flags = 65535;
  nested_rows[0].point.minimum = -1; nested_rows[2].wide = 0x123456789abcdef0ULL;
}
extern "C" unsigned ri_independent() { return table[0].token == 19 && independent[0].token == 65535; }
extern "C" unsigned ri_tls_snapshot() { return row_hash(tls_rows, 3) ^ row_hash(p, 1) ^ row_hash(storage, 1); }
extern "C" void ri_tls_mutate() {
  tls_rows[0].token = 9; tls_rows[2].text = "last thread";
  p[0].token = 11; storage[0].flags = 13;
}
extern "C" unsigned ri_local(unsigned seed) {
  TokenRow local[3] = {{"local", 5, 65535, 32768}, {"prefix-tail" + 7, 4, 17, 9}};
  TokenRow copy[3] = {{"local", 5, 65535, 32768}, {"prefix-tail" + 7, 4, 17, 9}};
  local[0].token = static_cast<unsigned short>(seed);
  local[2].length = static_cast<unsigned short>(seed >> 16);
  return row_hash(local, 3) ^ row_hash(copy, 3);
}
