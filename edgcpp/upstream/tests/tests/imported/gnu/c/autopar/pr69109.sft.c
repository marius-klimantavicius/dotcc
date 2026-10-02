//type: fp
//options: 
# 0 "./autopar/pr69109.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./autopar/pr69109.c"



# 1 "./autopar/../vect/unswitch-loops-pr26969.c" 1



void
ruby_re_compile_fastmap (char *fastmap, int options)
{
  int j;
  for (j = 0; j < (1 << 8); j++)
    {
      if (j != '\n' || (options & 4))
 fastmap[j] = 1;
    }
}
# 5 "./autopar/pr69109.c" 2
