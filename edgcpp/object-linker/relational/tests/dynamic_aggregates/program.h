extern int calls;
struct Pair { int first; int second; };
struct Nested { Pair pair; int tail; };
struct View { int *data; int size; };
int mark(int value);
Pair make_pair(int value);
View make_view(int *data, int size);
