//type: fp
//options: 
# 0 "./missing-header-fixit-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./missing-header-fixit-5.c"







int
foo (char *m, int i)
{
  if (isdigit (m[0]))
# 20 "./missing-header-fixit-5.c"
    {
      return abs (i);
# 30 "./missing-header-fixit-5.c"
    }
  else
    putchar (m[0]);
# 41 "./missing-header-fixit-5.c"
  return i;
}
