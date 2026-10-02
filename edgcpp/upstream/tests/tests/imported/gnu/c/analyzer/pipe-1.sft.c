//type: fp
//options: 
# 0 "./analyzer/pipe-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pipe-1.c"


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
# 4 "./analyzer/pipe-1.c" 2

extern int pipe(int pipefd[2]);
extern int close(int fd);

void
test_leak (void)
{
  int fds[2];
  if (pipe (fds) == -1)

    return;
}





void
test_close (void)
{
  int fds[2];
  if (pipe (fds) == -1)
    return;
  __analyzer_describe (0, fds[0]);
  __analyzer_describe (0, fds[1]);
  close (fds[0]);
  close (fds[1]);
}

void
test_unchecked (void)
{
  int fds[2];
  pipe (fds);
  close (fds[0]);
  close (fds[1]);
}
