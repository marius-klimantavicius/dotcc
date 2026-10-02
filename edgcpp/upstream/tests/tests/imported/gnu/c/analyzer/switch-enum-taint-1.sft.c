//type: fp
//options: 
# 0 "./analyzer/switch-enum-taint-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/switch-enum-taint-1.c"
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
# 2 "./analyzer/switch-enum-taint-1.c" 2



enum e
{
 E_VAL0,
 E_VAL1,
 E_VAL2
};





int __attribute__((tainted_args))
test_all_values_covered_implicit_default_1 (enum e x)
{
  switch (x)
    {
    case E_VAL0:
      return 1066;
    case E_VAL1:
      return 1776;
    case E_VAL2:
      return 1945;
    }
  __analyzer_dump_path ();
}

int __attribute__((tainted_args))
test_all_values_covered_implicit_default_2 (enum e x)
{
  int result;
  switch (x)
    {
    case E_VAL0:
      result = 1066;
      break;
    case E_VAL1:
      result = 1776;
      break;
    case E_VAL2:
      result = 1945;
      break;
    }
  return result;
}



int __attribute__((tainted_args))
test_all_values_covered_explicit_default_1 (enum e x)
{
  switch (x)
    {
    case E_VAL0:
      return 1066;
    case E_VAL1:
      return 1776;
    case E_VAL2:
      return 1945;
    default:
      __analyzer_dump_path ();
      return 0;
    }
}

int __attribute__((tainted_args))
test_missing_values_explicit_default_1 (enum e x)
{
  switch (x)
    {
    default:
    case E_VAL0:
      return 1066;
    case E_VAL1:
      return 1776;
    }
  __analyzer_dump_path ();
  return 0;
}

int __attribute__((tainted_args))
test_missing_values_explicit_default_2 (enum e x)
{
  switch (x)
    {
    case E_VAL0:
      return 1066;
    case E_VAL1:
      return 1776;
    default:
      __analyzer_dump_path ();
      return 1945;
    }
  __analyzer_dump_path ();
  return 0;
}
