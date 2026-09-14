#include <stdio.h>
#include <string.h>
#include <uchar.h>

static const char rows[3][4] = {"ax", "b"};
static const char names[2][2][4] = {{"ax", "bx"}, {"cx", "dx"}};
static const char deep[2][2][2][4] = {{{"one", "two"}, {"", "tri"}}, {{"end"}}};
static const unsigned char exact[2][3] = {"abc", "def"};
static const char mixed[2][4] = {"hi", {'b', 'y', 'e'}};
static const char16_t wide[2][4] = {u"Ω", u"ab"};
static const char32_t full[2][3] = {U"😀", U"z"};
static const char *pointers[2] = {"pointer", "array"};
struct Field { char values[2][4]; };

int main(void) {
  char local[2][2][4] = {{"ab", "cd"}, {"ef"}};
  static char retained[2][4] = {"old", "new"};
  struct Field field = {{{"hi"}, {'b', 'y', 'e'}}};
  local[1][0][0] = 'E';
  printf("rows=%s,%s,%d names=%s,%s,%s,%s size=%zu\n",
         rows[0], rows[1], rows[2][0], names[0][0], names[0][1], names[1][0], names[1][1], sizeof names);
  printf("deep=%s,%s,%s zero=%d exact=%c%c%c,%c%c%c\n",
         deep[0][0][0], deep[0][0][1], deep[1][0][0], deep[1][1][1][3],
         exact[0][0], exact[0][1], exact[0][2], exact[1][0], exact[1][1], exact[1][2]);
  printf("mixed=%s,%s field=%s,%s local=%s,%s,%d retained=%s\n",
         mixed[0], mixed[1], field.values[0], field.values[1], local[0][1], local[1][0], local[1][1][0], retained[1]);
  printf("wide=%x,%x full=%x,%x pointers=%s,%s zeros=%d\n",
         (unsigned)wide[0][0], (unsigned)wide[1][1], (unsigned)full[0][0], (unsigned)full[1][0],
         pointers[0], pointers[1], names[1][1][3] + rows[0][3] + wide[0][3] + full[0][2]);
  return strcmp(local[1][0], "Ef") != 0;
}
