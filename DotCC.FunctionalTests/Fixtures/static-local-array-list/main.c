#include <stdio.h>

int *storage(int index) {
 static int first[3], second[2][2];
 return index ? &second[0][0] : first;
}
int tick(void) {
 static int count, values[2] = {3, 4};
 ++count;
 values[1] += count;
 return values[0] + values[1];
}
int separate(void) {
 static int first[3], second[4];
 return first[0] + second[0];
}
int initialized(void) {
 struct Cell { int value; };
 static struct Cell cells[2] = {{7}, {8}}, tail[2] = {{9}};
 static const char *words[2] = {"left", "right"}, *more[1] = {"tail"};
 return cells[1].value + tail[0].value + tail[1].value + words[0][0] + more[0][0];
}
int main(void) {
 int *a = storage(0), *b = storage(1);
 printf("zero=%d,%d distinct=%d\n", a[0], b[3], a != b);
 a[0] = 41; b[3] = 99;
 printf("retain=%d,%d separate=%d initialized=%d\n", storage(0)[0], storage(1)[3], separate(), initialized());
 int first = tick(), second = tick();
 printf("ticks=%d,%d\n", first, second);
 return 0;
}
