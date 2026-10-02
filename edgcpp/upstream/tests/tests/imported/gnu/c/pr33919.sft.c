//type: rp
//options: 
# 0 "./pr33919.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr33919.c"




# 1 "./pr33919-1.h" 1
# 1 "./pr33919-2.h" 1
char *nested_inc_base_file = "./pr33919.c";
# 2 "./pr33919-1.h" 2
char *inc_base_file = "./pr33919.c";
# 6 "./pr33919.c" 2

const char *base_file = "./pr33919.c";

extern int strcmp (const char *, const char *);
extern void abort (void);

int
main ()
{
  if (!strcmp (pre_inc_base_file, "<command line>"))
    abort ();
  if (strcmp (pre_inc_base_file, "./pr33919.c"))
    abort ();
  if (strcmp (base_file, "./pr33919.c"))
    abort ();
  if (strcmp (inc_base_file, "./pr33919.c"))
    abort ();
  if (strcmp (nested_inc_base_file, "./pr33919.c"))
    abort ();
  return 0;
}
