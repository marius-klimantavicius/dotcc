//type: fp
//options: 
# 0 "./compat/sdata-1_x.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/sdata-1_x.c"
# 1 "./compat/sdata-section.h" 1






extern void abort (void);
# 2 "./compat/sdata-1_x.c" 2

struct s { int x; int y[4]; };
extern struct s small_struct ;


int *xaddr (void) { return &small_struct.x; }
int *yaddr (int i) { return &small_struct.y[i]; }

void sdata_1_x (void)
{
  int i;


  small_struct.x = 5;
  for (i = 0; i < 4; i++)
    small_struct.y[i] = i + 42;

  if (*xaddr () != 5)
    abort ();

  for (i = 0; i < 4; i++)
    if (*yaddr (i) != i + 42)
      abort ();
}
