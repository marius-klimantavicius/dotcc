#include <stdio.h>
int plus(int x) { return x+2; }
int times(int x) { return x*3; }
int answer(void) { return 42; }
int main(void) { int (*table[3])(int)={plus,times}; int (*implicit[])(int)={times,plus};
 int (*const fixed[2])(int)={plus,times}; int (*noargs[1])(void)={answer};
 int (*empty[2])(int); empty[0]=plus; empty[1]=times;
 int (*matrix[2][2])(int)={{plus,times},{times,plus}};
 int data[2][3]={{1,2,3},{4,5,6}}; int (*row)[3]=data;
 printf("%d %d %d %d %d %d %d %d %d\n",table[0](3),table[1](4),table[2]==0,implicit[0](2),fixed[1](3),noargs[0](),empty[1](2),matrix[1][1](5),row[1][2]);return 0; }
