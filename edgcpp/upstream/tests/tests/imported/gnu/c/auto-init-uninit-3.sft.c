//type: fp
//options: 
# 0 "./auto-init-uninit-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-3.c"




# 1 "./uninit-3.c" 1





extern void error (char *);

int
parse_charconst (const char *start, const char *end)
{
  int c;
  int nchars, retval;

  nchars = 0;
  retval = 0;
  while (start < end)
    {
      c = *start++;
      if (c == '\'')
 break;
      nchars++;
      retval += c;
      retval <<= 8;
    }

  if (nchars == 0)
    return 0;

  if (c != '\'')
    error ("malformed character constant");

  return retval;
}
# 6 "./auto-init-uninit-3.c" 2
