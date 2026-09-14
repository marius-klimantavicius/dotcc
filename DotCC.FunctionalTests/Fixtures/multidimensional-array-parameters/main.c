#include <stdio.h>
static int sized(const unsigned char [2][2][3]);
static int unsized(int [][3]);
static int sized(const unsigned char rows[2][2][3]) {
  int result = rows[1][0][2];
  rows++;
  return result + rows[0][1][1] + (int)sizeof(*rows);
}
static int unsized(int rows[][3]) {
  rows[1][2] = 17;
  return (int)sizeof(*rows) + rows[0][1];
}
int main(void) {
  const unsigned char cube[2][2][3] = {{{1,2,3},{4,5,6}},{{7,8,9},{10,11,12}}};
  int grid[2][3] = {{1,2,3},{4,5,6}};
  int a = sized(cube), b = unsized(grid);
  printf("sized=%d unsized=%d write=%d\n",a,b,grid[1][2]);
  return 0;
}
