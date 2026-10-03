// Byte initialization must preserve storage size, encoding and independent copies.
static char greeting[24] = "hello world";
static char unicode_text[] = "caf\xc3\xa9 \xe2\x98\x83";
static char escaped_text[] = "quote \" slash \\ tab\t line\nreturn\r \xe2\x80\xa8 \xe2\x80\xa9 \xef\xbb\xbf \x01\x1f";
static char embedded[] = "a\0b\0c";
static unsigned char binary_data[12] = {0, 255, 128, 192, 175, 237, 160, 128};
static signed char signed_data[8] = {-1, -128, 0, 127, 42};
static unsigned char printable_data[8] = {65, 66, 67, 68};
static char duplicate_a[] = "independent";
static char duplicate_b[] = "independent";
// Force a bulk-copy chunk boundary to fall inside a three-byte UTF-8 scalar.
#define BD_UTF8_1 "\xe2\x98\x83"
#define BD_UTF8_2 BD_UTF8_1 BD_UTF8_1
#define BD_UTF8_4 BD_UTF8_2 BD_UTF8_2
#define BD_UTF8_8 BD_UTF8_4 BD_UTF8_4
#define BD_UTF8_16 BD_UTF8_8 BD_UTF8_8
#define BD_UTF8_32 BD_UTF8_16 BD_UTF8_16
#define BD_UTF8_64 BD_UTF8_32 BD_UTF8_32
#define BD_UTF8_128 BD_UTF8_64 BD_UTF8_64
#define BD_UTF8_256 BD_UTF8_128 BD_UTF8_128
#define BD_UTF8_512 BD_UTF8_256 BD_UTF8_256
#define BD_UTF8_1024 BD_UTF8_512 BD_UTF8_512
#define BD_UTF8_2048 BD_UTF8_1024 BD_UTF8_1024
#define BD_UTF8_4096 BD_UTF8_2048 BD_UTF8_2048
#define BD_UTF8_8192 BD_UTF8_4096 BD_UTF8_4096
static char chunked_utf8[] = BD_UTF8_8192;
static char16_t wide16[] = u"wide \u2603 \U0001f600";
static char32_t wide32[] = U"wide \u2603 \U0001f600";
static thread_local char tls_text[20] = "thread caf\xc3\xa9";
static thread_local unsigned char tls_binary[8] = {255, 0, 128, 65};

static unsigned checksum(const void *value, unsigned size) {
  const unsigned char *bytes = static_cast<const unsigned char *>(value);
  unsigned result = 2166136261u;
  for (unsigned i = 0; i != size; ++i) result = (result ^ bytes[i]) * 16777619u;
  return result;
}

extern "C" unsigned bd_snapshot() {
  return checksum(greeting, sizeof(greeting)) ^ checksum(unicode_text, sizeof(unicode_text))
       ^ checksum(escaped_text, sizeof(escaped_text)) ^ checksum(embedded, sizeof(embedded))
       ^ checksum(binary_data, sizeof(binary_data))
       ^ checksum(signed_data, sizeof(signed_data)) ^ checksum(printable_data, sizeof(printable_data))
       ^ checksum(duplicate_a, sizeof(duplicate_a)) ^ (checksum(duplicate_b, sizeof(duplicate_b)) * 3u)
       ^ checksum(chunked_utf8, sizeof(chunked_utf8))
       ^ checksum(wide16, sizeof(wide16)) ^ checksum(wide32, sizeof(wide32));
}

extern "C" void bd_mutate() {
  greeting[0] = 'H';
  greeting[23] = '!';
  binary_data[11] = 254;
  signed_data[7] = -5;
  duplicate_a[0] = 'I';
}

extern "C" unsigned bd_independent() {
  return duplicate_a[0] == 'I' && duplicate_b[0] == 'i';
}

extern "C" unsigned bd_tls_snapshot() {
  return checksum(tls_text, sizeof(tls_text)) ^ checksum(tls_binary, sizeof(tls_binary));
}

extern "C" void bd_tls_mutate() {
  tls_text[0] = 'T';
  tls_text[19] = '?';
  tls_binary[7] = 42;
}

extern "C" unsigned bd_local(unsigned seed) {
  char first[18] = "local \xe2\x98\x83";
  char second[18] = "local \xe2\x98\x83";
  unsigned char binary[9] = {0, 255, 128, 192, 175};
  signed char negative[5] = {-1, -128, 42};
  char internal_null[7] = "a\0b";
  first[0] = static_cast<char>(seed);
  first[17] = static_cast<char>(seed >> 8);
  binary[8] = static_cast<unsigned char>(seed);
  return checksum(first, sizeof(first)) ^ checksum(second, sizeof(second))
       ^ checksum(binary, sizeof(binary)) ^ checksum(negative, sizeof(negative))
       ^ checksum(internal_null, sizeof(internal_null));
}

extern "C" const char *bd_literal(int index) {
  switch (index) {
    case 0: return "readable literal caf\xc3\xa9";
    case 1: return "comment */ marker /* quote \" slash \\ newline\n next";
    case 2: return "inside\0after";
    case 3: return "invalid \xc0\xaf";
    case 4: return "long literal abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789 repeated abcdefghijklmnopqrstuvwxyz trailing";
    default: return "";
  }
}

extern "C" unsigned bd_literals() {
  return checksum(bd_literal(0), sizeof("readable literal caf\xc3\xa9"))
       ^ checksum(bd_literal(1), sizeof("comment */ marker /* quote \" slash \\ newline\n next"))
       ^ checksum(bd_literal(2), sizeof("inside\0after"))
       ^ checksum(bd_literal(3), sizeof("invalid \xc0\xaf"))
       ^ checksum(bd_literal(4), sizeof("long literal abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789 repeated abcdefghijklmnopqrstuvwxyz trailing"))
       ^ checksum(bd_literal(5), 1);
}
