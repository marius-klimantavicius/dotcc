static const char *names[7] = {"alpha", "repeat", "repeat", "prefix-tail" + 7, nullptr};
static const char *name_copies[3] = {"alpha", "repeat", "repeat"};
extern int values[4];
static int *addresses[5] = {&values[3], &values[0], &values[3], nullptr};
int values[4] = {17, -23, 42, 99};
extern void *cyclic[3];
void *cyclic[3] = {&cyclic[1], &cyclic[2], &cyclic[0]};
static int add_three(int value) { return value + 3; }
static int subtract_two(int value) { return value - 2; }
typedef int (*Operation)(int);
static Operation operations[5] = {add_three, subtract_two, add_three, nullptr};
static unsigned words[7] = {1116352408u, 1899447441u, 3049323471u, 3921009573u};
static char message[18] = "byte table";
static int matrix[3][4] = {{1, 2}, {-3, 4, 5}, {6}};
static int zeros[4] = {};
static int error[4] = {1, 2, 3};
struct Starter { Starter() { error[3] = 7; } };
static Starter starter;
union Word { unsigned value; float floating; };
static Word unions[3] = {{0xf1234567u}, {17}};
struct Bits { unsigned first : 3; signed second : 5; unsigned short tag; };
static Bits bit_rows[3] = {{7, -16, 65535}, {3, 15, 17}};
struct WithArray { unsigned short elements[3]; const char *name; };
static WithArray array_rows[3] = {{{1, 65535, 32768}, "array"}, {{7}, "partial"}};
static thread_local const char *p[4] = {"thread p", "repeat", "repeat"};
static thread_local unsigned storage[5] = {123u, 456u, 789u};
static thread_local Operation tls_operations[3] = {subtract_two, add_three};
union Target { Operation call; const char *text; unsigned number; };
struct Entry { const char *name; unsigned short kind; Target u; };
struct LogData { unsigned header; Entry tab[5]; int matrix[2][3]; const char *footer; };
static LogData log_data = {
  17,
  {{"function", 1, {.call = add_three}}, {"text", 2, {.text = "prefix-tail" + 7}},
   {"null", 3, {.call = nullptr}}, {"number", 4, {.number = 0xf1234567u}}},
  {{1, 2}, {-3, 4, 5}}, "footer"
};
static Target global_target = {.call = subtract_two};
static Word union_float = {.floating = -0.0f};
static thread_local LogData tls_log_data = {
  23,
  {{"thread function", 1, {.call = subtract_two}}, {"thread text", 2, {.text = "thread value"}},
   {"thread null", 3, {.call = nullptr}}, {"thread number", 4, {.number = 0xabcdef01u}}},
  {{7, 8}, {9}}, "thread footer"
};
static thread_local Target tls_target = {.text = "thread target"};

static unsigned mix(unsigned a, unsigned b) { return (a ^ b) * 16777619u; }
static unsigned text_hash(const char *text) {
  unsigned value = 2166136261u;
  if (!text) return 0;
  while (*text) value = mix(value, static_cast<unsigned char>(*text++));
  return value;
}
static unsigned log_hash(const LogData *log) {
  unsigned value = mix(2166136261u, log->header);
  for (unsigned i = 0; i != 5; ++i) {
    value = mix(value, text_hash(log->tab[i].name));
    value = mix(value, log->tab[i].kind);
    switch (log->tab[i].kind) {
      case 1: value = mix(value, log->tab[i].u.call(7)); break;
      case 2: value = mix(value, text_hash(log->tab[i].u.text)); break;
      case 3: value = mix(value, log->tab[i].u.call == nullptr); break;
      case 4: value = mix(value, log->tab[i].u.number); break;
    }
  }
  for (unsigned i = 0; i != 2; ++i)
    for (unsigned j = 0; j != 3; ++j) value = mix(value, static_cast<unsigned>(log->matrix[i][j]));
  return mix(value, text_hash(log->footer));
}
static bool number_padding_is_zero(const Target *value) {
  const unsigned char *bytes = reinterpret_cast<const unsigned char *>(value);
  for (unsigned i = sizeof(value->number); i != sizeof(Target); ++i)
    if (bytes[i] != 0) return false;
  return true;
}
extern "C" unsigned ta_snapshot() {
  unsigned value = 2166136261u;
  for (unsigned i = 0; i != 7; ++i) value = mix(value, text_hash(names[i]));
  for (unsigned i = 0; i != 3; ++i) value = mix(value, text_hash(name_copies[i]));
  for (unsigned i = 0; i != 7; ++i) value = mix(value, words[i]);
  for (unsigned i = 0; i != 18; ++i) value = mix(value, static_cast<unsigned char>(message[i]));
  for (unsigned i = 0; i != 4; ++i) value = mix(value, static_cast<unsigned>(values[i] + zeros[i] + error[i]));
  for (unsigned i = 0; i != 3; ++i)
    for (unsigned j = 0; j != 4; ++j) value = mix(value, static_cast<unsigned>(matrix[i][j]));
  return value ^ log_hash(&log_data) ^ static_cast<unsigned>(global_target.call(7));
}
extern "C" unsigned ta_fallback() {
  return names[3][0] == 't' && names[3][3] == 'l' && names[4] == nullptr
      && names[5] == nullptr && names[6] == nullptr
      && addresses[0] == &values[3] && addresses[1] == &values[0] && addresses[2] == &values[3]
      && addresses[3] == nullptr && addresses[4] == nullptr
      && cyclic[0] == &cyclic[1] && cyclic[1] == &cyclic[2] && cyclic[2] == &cyclic[0]
      && operations[0](17) == 20 && operations[1](17) == 15 && operations[2](17) == 20
      && operations[3] == nullptr && operations[4] == nullptr
      && unions[0].value == 0xf1234567u && unions[1].value == 17 && unions[2].value == 0
      && bit_rows[0].first == 7 && bit_rows[0].second == -16 && bit_rows[0].tag == 65535
      && bit_rows[1].first == 3 && bit_rows[1].second == 15 && bit_rows[2].tag == 0
      && array_rows[0].elements[1] == 65535 && array_rows[0].elements[2] == 32768
      && array_rows[1].elements[0] == 7 && array_rows[1].elements[1] == 0
      && array_rows[2].elements[0] == 0 && array_rows[2].name == nullptr && error[3] == 7
      && log_data.tab[0].u.call(7) == 10 && log_data.tab[1].u.text[0] == 't'
      && log_data.tab[2].u.call == nullptr && log_data.tab[3].u.number == 0xf1234567u
      && log_data.tab[4].name == nullptr && log_data.tab[4].kind == 0 && log_data.tab[4].u.call == nullptr
      && global_target.call(7) == 5 && number_padding_is_zero(&log_data.tab[3].u)
      && reinterpret_cast<const unsigned char *>(&union_float)[0] == 0
      && reinterpret_cast<const unsigned char *>(&union_float)[1] == 0
      && reinterpret_cast<const unsigned char *>(&union_float)[2] == 0
      && reinterpret_cast<const unsigned char *>(&union_float)[3] == 128;
}
extern "C" void ta_mutate() {
  names[0] = "changed"; words[6] = 0xffffffffu; message[17] = '!';
  values[2] = -123; matrix[2][3] = 91; zeros[3] = 42;
  log_data.header = 41; log_data.tab[0].name = "changed function"; log_data.matrix[1][2] = -99;
}
extern "C" unsigned ta_independent() {
  return names[0][0] == 'c' && name_copies[0][0] == 'a';
}
extern "C" unsigned ta_tls_snapshot() {
  unsigned value = 2166136261u;
  for (unsigned i = 0; i != 4; ++i) value = mix(value, text_hash(p[i]));
  for (unsigned i = 0; i != 5; ++i) value = mix(value, storage[i]);
  value = mix(value, tls_operations[0](17));
  value = mix(value, tls_operations[1](17));
  return mix(value, tls_operations[2] == nullptr) ^ log_hash(&tls_log_data) ^ text_hash(tls_target.text);
}
extern "C" void ta_tls_mutate() {
  p[0] = "changed thread"; storage[4] = 0xf1234567u;
  tls_log_data.header = 71; tls_log_data.tab[0].u.call = add_three; tls_log_data.matrix[1][2] = 19;
  tls_target.text = "changed target";
}
extern "C" unsigned ta_local(unsigned seed) {
  const char *local[4] = {"first", "second", "third"};
  Operation local_ops[3] = {add_three, subtract_two};
  local[0] = "changed";
  return text_hash(local[0]) ^ text_hash(local[1]) ^ text_hash(local[2])
      ^ static_cast<unsigned>(local_ops[0](static_cast<int>(seed)))
      ^ static_cast<unsigned>(local_ops[1](static_cast<int>(seed))) ^ (local[3] == nullptr);
}
