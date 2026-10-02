//options_all:-r -x -tused
//options: --strict;cp

// Base type for bit fields in generated C must match assumptions
// made by layout in front end.
struct st2 { char :2; };
struct S {
  st2 x;
  st2 x2;
} s;
extern "C" int printf(const char *, ...);
int main () {
  char *p = (char *)&s.x;
  char *p2 = (char *)&s.x2;
  int diff = p2 - p;
  printf("sizeof = %d, computed = %d\n", sizeof(st2), diff);
  return !(sizeof(st2) == diff);
}

