struct point { const char *name; int count; };
static int count(void) { static struct point point = {.name = "log", .count = 7}; return point.count++; }
int main(void) { return count() + count() - 15; }
