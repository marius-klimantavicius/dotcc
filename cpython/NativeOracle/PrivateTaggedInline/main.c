#include <stdio.h>
int first_value(void); int second_value(void);
int main(void) { printf("%d %d\n",first_value(),second_value()); return first_value()==7 && second_value()==42 ? 0:1; }
