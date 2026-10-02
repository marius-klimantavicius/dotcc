//type: rp
//options: 
# 0 "./pr68670-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr68670-1.c"




# 1 "./../gcc.c-torture/execute/pr68376-1.c" 1


int a, b, c = 1;
signed char d;

int
main ()
{
  for (; a < 1; a++)
    for (; b < 1; b++)
      {
 signed char e = ~d;
 if (d < 1)
   e = d;
 d = e;
 if (!c)
   __builtin_abort ();
      }

  if (d != 0)
    __builtin_abort ();

  return 0;
}
# 6 "./pr68670-1.c" 2
