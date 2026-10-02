//type: fp
//options: 
# 0 "./analyzer/taint-merger.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/taint-merger.c"



# 1 "./analyzer/analyzer-decls.h" 1







extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 32 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);
# 5 "./analyzer/taint-merger.c" 2

int v_start;

__attribute__((tainted_args))
void test (int v_tainted, int v_has_lb, int v_has_ub, int v_stop)
{

  if (v_has_lb < 10)
    return;
  if (v_has_ub > 100)
    return;
  if (v_stop < 0 || v_stop > 100)
    return;



  __analyzer_dump_state ("taint", v_start);
  __analyzer_dump_state ("taint", v_tainted);
  __analyzer_dump_state ("taint", v_has_lb);
  __analyzer_dump_state ("taint", v_has_ub);
  __analyzer_dump_state ("taint", v_stop);


  __analyzer_dump_state ("taint", v_start + v_start);
  __analyzer_dump_state ("taint", v_start + v_tainted);
  __analyzer_dump_state ("taint", v_start + v_has_lb);
  __analyzer_dump_state ("taint", v_start + v_has_ub);
  __analyzer_dump_state ("taint", v_start + v_stop);

  __analyzer_dump_state ("taint", v_tainted + v_start);
  __analyzer_dump_state ("taint", v_tainted + v_tainted);
  __analyzer_dump_state ("taint", v_tainted + v_has_lb);
  __analyzer_dump_state ("taint", v_tainted + v_has_ub);
  __analyzer_dump_state ("taint", v_tainted + v_stop);

  __analyzer_dump_state ("taint", v_has_lb + v_start);
  __analyzer_dump_state ("taint", v_has_lb + v_tainted);
  __analyzer_dump_state ("taint", v_has_lb + v_has_lb);
  __analyzer_dump_state ("taint", v_has_lb + v_has_ub);
  __analyzer_dump_state ("taint", v_has_lb + v_stop);

  __analyzer_dump_state ("taint", v_has_ub + v_start);
  __analyzer_dump_state ("taint", v_has_ub + v_tainted);
  __analyzer_dump_state ("taint", v_has_ub + v_has_lb);
  __analyzer_dump_state ("taint", v_has_ub + v_has_ub);
  __analyzer_dump_state ("taint", v_has_ub + v_stop);

  __analyzer_dump_state ("taint", v_stop + v_start);
  __analyzer_dump_state ("taint", v_stop + v_tainted);
  __analyzer_dump_state ("taint", v_stop + v_has_lb);
  __analyzer_dump_state ("taint", v_stop + v_has_ub);
  __analyzer_dump_state ("taint", v_stop + v_stop);
}
