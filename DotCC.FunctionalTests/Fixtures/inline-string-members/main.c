#include <stdio.h>
#include <uchar.h>

struct Transform { unsigned char nName; char zName[7]; float scale; };
static const struct Transform transforms[] = {
  {6, "second", 1.0}, {3, "day", 86400.0}
};
struct Text {
  char exact[3];
  char padded[7];
  unsigned char escaped[4];
  char utf8[3];
  char embedded[4];
};
static struct Text global = {"abc", {"day"}, "\xff\101", "λ", "A\0B"};
struct Wrapper { struct Transform entries[2]; char tail[3]; };
static struct Wrapper nested = {{{4, "hour", 3600.0}, {4, "year", 31536000.0}}, "xy"};
struct Words { char values[3][4]; };
static struct Words words = {{"one", "two"}};
struct Wide { char16_t short_text[2]; char32_t long_text[2]; };
static struct Wide wide = {u"λ", U"😀"};

int main(void) {
  struct Text local = {"xyz", "a", "\x80" "B", "\xce\xbb", "C\0D"};
  local.exact[0] = 'q';
  printf("%d %d %d %d\n", transforms[0].nName, transforms[0].zName[5],
    transforms[0].zName[6], transforms[1].zName[6]);
  printf("%d %d %d %d %d\n", global.exact[2], global.padded[2],
    global.padded[6], global.escaped[0], global.escaped[1]);
  printf("%d %d %d %d %d\n", (unsigned char)global.utf8[0],
    (unsigned char)global.utf8[1], global.utf8[2], global.embedded[2], global.embedded[3]);
  printf("%d %d %d %d %d %d\n", local.exact[0], global.exact[0],
    local.escaped[0], local.escaped[1], (unsigned char)local.utf8[1], local.embedded[2]);
  printf("%d %d %d %d\n", nested.entries[0].zName[3], nested.entries[1].zName[6],
    nested.tail[2], (int)sizeof(local));
  printf("%d %d %d %d\n", words.values[0][2], words.values[1][2], words.values[1][3], words.values[2][0]);
  printf("%u %u %u %u\n", (unsigned)wide.short_text[0], (unsigned)wide.short_text[1],
    (unsigned)wide.long_text[0], (unsigned)wide.long_text[1]);
  return 0;
}
