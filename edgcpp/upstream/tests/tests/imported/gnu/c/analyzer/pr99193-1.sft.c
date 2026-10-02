//type: fp
//options: 
# 0 "./analyzer/pr99193-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr99193-1.c"
# 12 "./analyzer/pr99193-1.c"
typedef long unsigned int size_t;
typedef __builtin_va_list va_list;

# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 16 "./analyzer/pr99193-1.c" 2

extern void *malloc (size_t __size)
  __attribute__ ((__nothrow__ , __leaf__))
  __attribute__ ((__malloc__))
  __attribute__ ((__alloc_size__ (1)));
extern void perror (const char *__s);
extern void *realloc (void *__ptr, size_t __size)
  __attribute__ ((__nothrow__ , __leaf__))
  __attribute__ ((__warn_unused_result__))
  __attribute__ ((__alloc_size__ (2)));

extern void guestfs_int_cleanup_free (void *ptr);
extern int commandrvf (char **stdoutput, char **stderror, unsigned flags,
                       char const* const *argv);


int
commandrf (char **stdoutput, char **stderror, unsigned flags,
           const char *name, ...)
{
  va_list args;
  __attribute__((cleanup(guestfs_int_cleanup_free))) const char **argv = ((void *)0);
  char *s;
  int i, r;


  i = 2;
  argv = (const char **) malloc (sizeof (char *) * i);

 if (argv == ((void *)0)) {
    perror ("malloc");
    return -1;
  }
  argv[0] = (char *) name;
  argv[1] = ((void *)0);

  __builtin_va_start (args, name);

  while ((s = __builtin_va_arg (args, char *)) != ((void *)0)) {
    const char **p = (const char **) realloc (argv, sizeof (char *) * (++i));
    if (p == ((void *)0)) {
      perror ("realloc");
      __builtin_va_end (args);
      return -1;
    }
    argv = p;
    argv[i-2] = s;
    argv[i-1] = ((void *)0);
  }

  __builtin_va_end (args);

  r = commandrvf (stdoutput, stderror, flags, argv);

  return r;
}
