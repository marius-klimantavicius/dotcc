//type: fp
//options: 
# 0 "./analyzer/pr93355-localealias-feasibility-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr93355-localealias-feasibility-3.c"
# 27 "./analyzer/pr93355-localealias-feasibility-3.c"
typedef long unsigned int size_t;


typedef struct _IO_FILE FILE;
extern FILE *fopen (const char *__restrict __filename,
      const char *__restrict __modes);
extern int fclose (FILE *__stream);

extern int isspace (int) __attribute__((__nothrow__, __leaf__));



size_t
read_alias_file (const char *fname, char *cp)
{
  FILE *fp;

  fp = fopen (fname, "r");
  if (fp == ((void *)0))
    return 0;

  if (cp[0] != '\0')
    *cp++ = '\0';

  while (isspace ((unsigned char)cp[0]))
    ++cp;

  if (cp[0] != '\0')
    return 42;

  fclose(fp);

  return 0;
}
