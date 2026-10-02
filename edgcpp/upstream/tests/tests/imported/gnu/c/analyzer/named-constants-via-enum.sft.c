//type: fp
//options: 
# 0 "./analyzer/named-constants-via-enum.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/named-constants-via-enum.c"
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
# 2 "./analyzer/named-constants-via-enum.c" 2


enum {
      O_ACCMODE = 42,
      O_RDONLY = 0x1,
      O_WRONLY = 010
};

void test_sm_fd_constants (void)
{
  __analyzer_dump_named_constant ("O_ACCMODE");
  __analyzer_dump_named_constant ("O_RDONLY");
  __analyzer_dump_named_constant ("O_WRONLY");
}

void test_unknown (void)
{
  __analyzer_dump_named_constant ("UNKNOWN");
}
