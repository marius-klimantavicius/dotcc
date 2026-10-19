#include <stdio.h>
struct Cell { int value; };
struct Graph { char cookie[8]; unsigned short wide[4]; int matrix[2][3]; struct Cell cells[2]; int tail; };
static struct Graph graph = { .cookie=("xdebugpy"), .wide={1,2}, .matrix={{3,4},{5}}, .cells={{.value=6},{7}}, .tail=8 };
int main(void) {
 struct Graph local = {.cookie=("a\0b"), .wide={9}, .matrix={{10},{11,12}}, .cells={{13}}, .tail=14};
 printf("%c%c %d %d %d %d %d %d %d %d %d %d %d %d\n",graph.cookie[0],graph.cookie[7],graph.wide[2],graph.matrix[0][1],graph.matrix[1][2],graph.cells[1].value,graph.tail,local.cookie[2],local.cookie[3],local.wide[0],local.matrix[1][1],local.cells[0].value,local.tail,(int)((char*)&graph.tail-(char*)&graph));
 return 0;
}
