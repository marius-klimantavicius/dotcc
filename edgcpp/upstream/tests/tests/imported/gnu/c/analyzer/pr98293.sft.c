//type: fp
//options: 
# 0 "./analyzer/pr98293.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr98293.c"

# 1 "./analyzer/../pr93399.c" 1




extern inline __attribute__ ((__always_inline__, __gnu_inline__)) char *
strstr (const char *haystack, const char *needle)
{
  return __builtin_strstr (haystack, needle);
}

int
main (int argc, const char **argv)
{
  char *substr = strstr (argv[0], "\n");
  char *another = strstr (argv[0], "\r\n");
  return 0;
}
# 3 "./analyzer/pr98293.c" 2
