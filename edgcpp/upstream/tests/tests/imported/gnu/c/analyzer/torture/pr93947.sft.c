//type: fp
//options: 
# 0 "./analyzer/torture/pr93947.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/pr93947.c"


# 1 "./analyzer/torture/../analyzer-decls.h" 1
# 20 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 4 "./analyzer/torture/pr93947.c" 2

struct pf {
  unsigned int iu : 2;
};

enum {
  qr, jv, vm, mz,
};

int uh;

void
w9 (struct pf *x2)
{

  __analyzer_eval (x2->iu >= 0 );
  __analyzer_eval (x2->iu < 4 );

  switch (x2->iu)
    {
    case qr:
    case jv:
    case vm:
      uh = 0;
      break;

    case mz:
      break;

    default:



      __analyzer_eval (x2->iu);
      __builtin_abort ();
    }
}
