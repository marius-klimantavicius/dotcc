static short s16[8] = {-32768, -1, 0, 1, 32767};
static unsigned short u16[8] = {0, 65535, 32768, 255, 256};
static int s32[8] = {-2147483647 - 1, -1, 0, 1, 2147483647};
static unsigned u32[8] = {1116352408u, 1899447441u, 3049323471u, 3921009573u,
                         961987163u, 1508970993u, 2453635748u, 2870763221u};
static long long s64[8] = {-9223372036854775807LL - 1, -1, 0, 1, 9223372036854775807LL};
static unsigned long long u64[8] = {0, 18446744073709551615ULL, 9223372036854775808ULL,
                                  1234567890123456789ULL, 1};
static _Float16 f16[8] = {0.0f, -0.0f, 1.5f, -2.25f, 0x1p-24f, 0x1p-14f,
                          __builtin_inff(), -__builtin_inff()};
static float f32[8] = {0.0f, -0.0f, 1.5f, -2.25f, __builtin_inff(), -__builtin_inff(),
                       __builtin_nanf("0x1234"), 0x1p-149f};
static double f64[8] = {0.0, -0.0, 1.5, -2.25, __builtin_inf(), -__builtin_inf(),
                        __builtin_nan("0x1234"), 0x1p-1074};
static bool flags[8] = {true, false, true, true};
enum Number : unsigned { low = 7, high = 0xf1234567u };
static Number enums[5] = {high, low, high};
static int grid[3][4] = {{1, 2}, {-3, 4, 5}, {6}};
static unsigned copy_a[4] = {1116352408u, 1899447441u, 3049323471u, 3921009573u};
static unsigned copy_b[4] = {1116352408u, 1899447441u, 3049323471u, 3921009573u};
#define PD_WORDS_4 0x12345678u, 0xabcdef01u, 0x80000000u, 0xffffffffu
#define PD_WORDS_8 PD_WORDS_4, PD_WORDS_4
#define PD_WORDS_16 PD_WORDS_8, PD_WORDS_8
#define PD_WORDS_32 PD_WORDS_16, PD_WORDS_16
#define PD_WORDS_64 PD_WORDS_32, PD_WORDS_32
#define PD_WORDS_128 PD_WORDS_64, PD_WORDS_64
#define PD_WORDS_256 PD_WORDS_128, PD_WORDS_128
#define PD_WORDS_512 PD_WORDS_256, PD_WORDS_256
#define PD_WORDS_1024 PD_WORDS_512, PD_WORDS_512
#define PD_WORDS_2048 PD_WORDS_1024, PD_WORDS_1024
#define PD_WORDS_4096 PD_WORDS_2048, PD_WORDS_2048
#define PD_ZEROS_4 0, 0, 0, 0
#define PD_ZEROS_8 PD_ZEROS_4, PD_ZEROS_4
#define PD_ZEROS_16 PD_ZEROS_8, PD_ZEROS_8
#define PD_ZEROS_32 PD_ZEROS_16, PD_ZEROS_16
#define PD_ZEROS_64 PD_ZEROS_32, PD_ZEROS_32
#define PD_ZEROS_128 PD_ZEROS_64, PD_ZEROS_64
#define PD_ZEROS_256 PD_ZEROS_128, PD_ZEROS_128
#define PD_ZEROS_512 PD_ZEROS_256, PD_ZEROS_256
#define PD_ZEROS_1024 PD_ZEROS_512, PD_ZEROS_512
#define PD_ZEROS_2048 PD_ZEROS_1024, PD_ZEROS_1024
#define PD_ZEROS_4096 PD_ZEROS_2048, PD_ZEROS_2048
#define PD_ZEROS_8192 PD_ZEROS_4096, PD_ZEROS_4096
static unsigned chunked[12304] = {PD_WORDS_4096, PD_WORDS_4, PD_ZEROS_8192, 7, 9, 11, 13};
static unsigned sparse[8192] = {0x12345678u};
static thread_local unsigned tls32[6] = {123u, 456u, 789u};
static thread_local long long tls64[4] = {-1, -9223372036854775807LL - 1, 77};
static unsigned *relocations[3] = {&u32[0], &u32[3], &u32[7]};
struct Relocated { int tag; unsigned *value; };
static Relocated records[3] = {{3, &u32[1]}, {-7, &u32[4]}, {19, &u32[6]}};

static unsigned hash(const void *value, unsigned size) {
  const unsigned char *bytes = static_cast<const unsigned char *>(value);
  unsigned result = 2166136261u;
  for (unsigned i = 0; i != size; ++i) result = (result ^ bytes[i]) * 16777619u;
  return result;
}

extern "C" unsigned pd_snapshot() {
  return hash(s16, sizeof(s16)) ^ hash(u16, sizeof(u16)) ^ hash(s32, sizeof(s32))
       ^ hash(u32, sizeof(u32)) ^ hash(s64, sizeof(s64)) ^ hash(u64, sizeof(u64))
       ^ hash(f16, sizeof(f16)) ^ hash(f32, sizeof(f32)) ^ hash(f64, sizeof(f64))
       ^ hash(flags, sizeof(flags))
       ^ hash(enums, sizeof(enums)) ^ hash(grid, sizeof(grid))
       ^ hash(copy_a, sizeof(copy_a)) ^ (hash(copy_b, sizeof(copy_b)) * 3u)
       ^ hash(chunked, sizeof(chunked)) ^ hash(sparse, sizeof(sparse));
}

extern "C" unsigned pd_floatbits() {
  return hash(f16, sizeof(f16)) ^ hash(f32, sizeof(f32)) ^ hash(f64, sizeof(f64));
}

extern "C" unsigned pd_fallback() {
  return relocations[0] == &u32[0] && relocations[1] == &u32[3] && relocations[2] == &u32[7]
      && records[0].tag == 3 && records[0].value == &u32[1]
      && records[1].tag == -7 && records[1].value == &u32[4]
      && records[2].tag == 19 && records[2].value == &u32[6]
      && *relocations[1] == 3921009573u && *records[1].value == 961987163u;
}

extern "C" void pd_mutate() {
  s16[7] = -123; u16[7] = 65534; s32[7] = -987654321; s64[7] = -12345678901234LL;
  u64[7] = 12345678901234ULL; grid[2][3] = 42; copy_a[0] = 9; f32[0] = 7.5f;
  chunked[4095] = 3; chunked[4096] = 4; chunked[4102] = 5;
  chunked[12292] = 6; chunked[12303] = 7; sparse[8191] = 8;
}

extern "C" unsigned pd_independent() { return copy_a[0] == 9 && copy_b[0] == 1116352408u; }
extern "C" unsigned pd_tls_snapshot() { return hash(tls32, sizeof(tls32)) ^ hash(tls64, sizeof(tls64)); }
extern "C" void pd_tls_mutate() { tls32[5] = 0xf1234567u; tls64[3] = 9223372036854775807LL; }

extern "C" unsigned pd_local(unsigned seed) {
  unsigned a[8] = {1116352408u, 1899447441u, 3049323471u, 3921009573u};
  unsigned b[8] = {1116352408u, 1899447441u, 3049323471u, 3921009573u};
  short c[3][4] = {{-32768, -1}, {1, 32767}, {3}};
  double d[6] = {-0.0, 1.5, -2.25, __builtin_inf()};
  a[7] = seed; c[2][3] = static_cast<short>(seed); d[5] = static_cast<double>(seed);
  return hash(a, sizeof(a)) ^ hash(b, sizeof(b)) ^ hash(c, sizeof(c)) ^ hash(d, sizeof(d));
}
