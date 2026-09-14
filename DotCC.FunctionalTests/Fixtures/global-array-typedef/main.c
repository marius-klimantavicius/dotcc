#include <stdio.h>

typedef unsigned long long slots[2];
typedef int matrix[2][3];
extern slots values;
slots values;
slots values;
static slots first, second;
static matrix grid;

int main(void) {
  unsigned long long zero = values[0] + values[1] + first[0] + second[1];
  values[1] = 42;
  first[0] = 7;
  second[1] = 9;
  grid[1][2] = 13;
  printf("zero=%llu value=%llu separate=%llu grid=%d sizes=%zu,%zu\n",
         zero, values[1], first[0] + second[1], grid[1][2], sizeof(values), sizeof(grid));
  return zero == 0 && values[1] == 42 && first[0] == 7 && second[1] == 9 && grid[1][2] == 13 ? 0 : 1;
}
