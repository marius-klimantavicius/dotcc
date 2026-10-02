//type: fp
//options: 
# 0 "./analyzer/pr93355-localealias-feasibility-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr93355-localealias-feasibility-2.c"






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
# 8 "./analyzer/pr93355-localealias-feasibility-2.c" 2




const char *
_nl_expand_alias (void)
{
  static const char *locale_alias_path;

  if (locale_alias_path == ((void *)0))
    locale_alias_path = "value for LOCALE_ALIAS_PATH";

  const char *start = locale_alias_path;

  while (locale_alias_path[0] != '\0'
  && locale_alias_path[0] != ':')
    ++locale_alias_path;

  if (start < locale_alias_path)
    __analyzer_dump_path ();
}
