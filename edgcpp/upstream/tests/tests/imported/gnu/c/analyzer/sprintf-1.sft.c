//type: fp
//options: 
# 0 "./analyzer/sprintf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/sprintf-1.c"




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
# 6 "./analyzer/sprintf-1.c" 2

extern int
sprintf(char* dst, const char* fmt, ...)
  __attribute__((__nothrow__));




int
test_uninit_fmt_buf (char *dst)
{
  const char fmt[10];
  return sprintf (dst, fmt);

}

int
test_fmt_not_terminated (char *dst)
{
  const char fmt[3] = "foo";
  return sprintf (dst, fmt);

}
