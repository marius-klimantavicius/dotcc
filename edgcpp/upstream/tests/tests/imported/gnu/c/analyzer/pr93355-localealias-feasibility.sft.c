//type: fp
//options: 
# 0 "./analyzer/pr93355-localealias-feasibility.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr93355-localealias-feasibility.c"
# 30 "./analyzer/pr93355-localealias-feasibility.c"
# 1 "./analyzer/../../gcc.dg/analyzer/analyzer-decls.h" 1
# 20 "./analyzer/../../gcc.dg/analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/../../gcc.dg/analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 31 "./analyzer/pr93355-localealias-feasibility.c" 2

typedef long unsigned int size_t;

typedef struct _IO_FILE FILE;
extern FILE *fopen (const char *__restrict __filename,
      const char *__restrict __modes);
extern size_t fread(void *ptr, size_t size, size_t nmemb, FILE *stream);
extern int fclose (FILE *__stream);

extern int isspace (int) __attribute__((__nothrow__, __leaf__));



size_t
read_alias_file (const char *fname, int fname_len)
{
  FILE *fp;
  size_t added;
  char buf[400];
  char *alias;
  char *value;
  char *cp;

  fp = fopen (fname, "r");
  if (fp == ((void *)0))
    return 0;

  if (fread (buf, sizeof buf, 1, fp) != 1)
    {
      fclose (fp);
      return 0;
    }

  cp = buf;


  while (isspace ((unsigned char)cp[0]))
    ++cp;

  if (cp[0] != '\0' && cp[0] != '#')
    {
      alias = cp++;
      while (cp[0] != '\0' && !isspace ((unsigned char)cp[0]))
 ++cp;
      if (cp[0] != '\0')
 *cp++ = '\0';

      while (isspace ((unsigned char)cp[0]))
 ++cp;

      if (cp[0] != '\0')
 return 42;
    }

  fclose(fp);

  return 0;
}
