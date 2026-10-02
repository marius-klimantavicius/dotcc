//type: rp
//options: --c89 --strict_gnu
# 0 "./builtin-dynamic-object-size-11.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-object-size-11.c"






# 1 "./builtin-object-size-11.c" 1





extern void abort (void);

struct s {
    int i;
    char c[];
} s = { 1, "01234" };

long unsigned int f (void) { return __builtin_dynamic_object_size (&s.c, 0); }

int
main()
{
  if (f() != sizeof ("01234"))
    abort ();

  return 0;
}
# 8 "./builtin-dynamic-object-size-11.c" 2
