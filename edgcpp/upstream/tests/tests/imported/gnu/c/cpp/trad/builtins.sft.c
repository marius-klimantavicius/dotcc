//type: rp
//options: 
# 0 "./cpp/trad/builtins.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp/trad/builtins.c"




# 1 "./cpp/trad/builtins.h" 1
int level = 1;
# 6 "./cpp/trad/builtins.c" 2

void abort (void);
char *strstr (const char *, const char *);
int strcmp (const char *, const char *);
# 39 "./cpp/trad/builtins.c"
int main ()
{

  if (level != 1)
    abort ();

  if (!strstr ("./cpp/trad/builtins.c", "builtins.c"))
    abort ();

  if (!strcmp ("./cpp/trad/builtins.c", "builtins.c"))
    abort ();

  return 0;
}
