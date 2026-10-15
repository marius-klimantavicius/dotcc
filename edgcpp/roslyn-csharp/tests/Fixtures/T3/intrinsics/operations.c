typedef unsigned char U8;
typedef unsigned short U16;
typedef unsigned int U32;
typedef unsigned long long U64;

static U8 bytes[3];
static U16 halves[3];
static U32 words[3];
static U64 quads[3];
static long long observations[40];
static int count;
static int trace;
static U32 ready;
static int payload;

static void observe(long long value) { observations[count] = value; count = count + 1; }
long long intrinsic_observation(int index) { return observations[index]; }
int intrinsic_count(void) { return count; }
int intrinsic_trace(void) { return trace; }

int fixture(void)
{
  long long output;
  long long maximum = 9223372036854775807LL;
  long long minimum = -9223372036854775807LL - 1;
  U16 swap16 = 0x1234;
  U32 swap32 = 0x12345678U;
  U64 swap64 = 0x0102030405060708ULL;
  U64 leading = 1;
  count = 0;
  trace = 0;
  bytes[0] = 17; bytes[2] = 51;
  halves[0] = 0x1111; halves[2] = 0x3333;
  words[0] = 0x11111111U; words[2] = 0x33333333U;
  quads[0] = 0x1111111111111111ULL; quads[2] = 0x3333333333333333ULL;
  __atomic_store_n(&bytes[1], (U8)211, __ATOMIC_RELAXED);
  __atomic_store_n(&halves[1], (U16)11121, __ATOMIC_RELAXED);
  __atomic_store_n(&words[1], (U32)305441741, __ATOMIC_RELAXED);
  __atomic_store_n(&quads[1], (U64)0x1234567812345678ULL, __ATOMIC_RELAXED);
  observe(__atomic_load_n(&bytes[1], __ATOMIC_RELAXED));
  observe(__atomic_load_n(&halves[1], __ATOMIC_RELAXED));
  observe(__atomic_load_n(&words[1], __ATOMIC_RELAXED));
  __sync_synchronize();
  observe((long long)quads[1]);
  observe(__builtin_bswap16(swap16));
  observe(__builtin_bswap32(swap32));
  observe((long long)__builtin_bswap64(swap64));
  observe(__builtin_clzll(leading));
  leading = 0x8000000000000000ULL;
  observe(__builtin_clzll(leading));
  leading = 0x0000010000000000ULL;
  observe(__builtin_clzll(leading));
  /* Comma keeps this intrinsic on the typed expression-plan rendering path. */
  observe((trace = trace + 1, __builtin_add_overflow(maximum, 1LL, &output)));
  observe(output);
  observe(__builtin_add_overflow(35LL, 7LL, &output)); observe(output);
  observe(__builtin_sub_overflow(minimum, 1LL, &output)); observe(output);
  observe(__builtin_sub_overflow(20LL, 7LL, &output)); observe(output);
  observe(__builtin_mul_overflow(maximum, 2LL, &output)); observe(output);
  observe(__builtin_mul_overflow(-7LL, 6LL, &output)); observe(output);
  output = maximum;
  observe(__builtin_add_overflow(output, 1LL, &output)); observe(output);
  observe(__builtin_mul_overflow(minimum, -1LL, &output)); observe(output);
  observe(bytes[0]); observe(bytes[2]); observe(halves[0]); observe(halves[2]);
  observe(words[0]); observe(words[2]); observe((long long)quads[0]); observe((long long)quads[2]);
  return count;
}

/* Full fences around captured relaxed flag accesses publish/acknowledge the
   non-atomic payload. The oracle and managed checker both verify every value. */
void intrinsic_publish(int value)
{
  while (__atomic_load_n(&ready, __ATOMIC_RELAXED) != 0) { }
  __sync_synchronize();
  payload = value;
  __sync_synchronize();
  __atomic_store_n(&ready, 1U, __ATOMIC_RELAXED);
}
int intrinsic_consume(void)
{
  int value;
  while (__atomic_load_n(&ready, __ATOMIC_RELAXED) == 0) { }
  __sync_synchronize();
  value = payload;
  __sync_synchronize();
  __atomic_store_n(&ready, 0U, __ATOMIC_RELAXED);
  return value;
}
