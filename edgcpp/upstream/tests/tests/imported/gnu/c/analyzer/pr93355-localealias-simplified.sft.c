//type: fp
//options: 
# 0 "./analyzer/pr93355-localealias-simplified.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr93355-localealias-simplified.c"
# 27 "./analyzer/pr93355-localealias-simplified.c"
typedef struct _IO_FILE FILE;
extern FILE *fopen(const char *__restrict __filename,
     const char *__restrict __modes);
extern int fclose(FILE *__stream);

void
read_alias_file (int flag)
{
  FILE *fp;

  fp = fopen ("name", "r");
  if (fp == ((void *) 0))
    return;

  if (flag)
    return;

  fclose (fp);
}
