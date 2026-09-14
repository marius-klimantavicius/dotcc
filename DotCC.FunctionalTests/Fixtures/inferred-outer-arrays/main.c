#include <stdio.h>

static const char names[][2][4] = {{"ax", "bx"}, {"cx", "dx"}};
static const unsigned char exact[][3] = {"abc", "def"};
int scalar[][3] = {{1, 2}, {3, 4, 5}, {6}};
static int flat[][2] = {1, 2, 3, 4, 5};
static const char *pointer_rows[][2] = {{"left", "right"}, {"tail"}};

int main(void) {
  char local[][2][4] = {{"one", "two"}, {"end"}};
  static char cached[][4] = {"old", "new"};
  local[1][0][0] = 'E';
  printf("names=%s,%s,%s,%s sizes=%zu,%zu,%zu,%zu\n", names[0][0], names[0][1], names[1][0], names[1][1], sizeof names, sizeof exact, sizeof scalar, sizeof flat);
  printf("exact=%c%c%c scalar=%d,%d flat=%d,%d local=%s,%d cached=%s\n", exact[1][0], exact[1][1], exact[1][2], scalar[1][2], scalar[2][2], flat[2][0], flat[2][1], local[1][0], local[1][1][0], cached[1]);
  printf("pointers=%s,%s,%s null=%d sizes=%zu,%zu\n", pointer_rows[0][0], pointer_rows[0][1], pointer_rows[1][0], pointer_rows[1][1] == NULL, sizeof local, sizeof cached);
  return 0;
}
