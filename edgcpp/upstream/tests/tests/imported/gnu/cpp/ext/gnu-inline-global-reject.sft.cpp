//type: fn
//options:  --c++03
# 0 "./ext/gnu-inline-global-reject.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/gnu-inline-global-reject.C"
# 9 "./ext/gnu-inline-global-reject.C"
# 1 "./ext/gnu-inline-common.h" 1
# 10 "./ext/gnu-inline-global-reject.C" 2



 inline int func_decl_inline_before (void);
 __attribute__((gnu_inline)) inline int func_decl_inline_before (void) { return 0; }



 __attribute__((gnu_inline)) inline int func_decl_inline_after (void) { return 0; }
 inline int func_decl_inline_after (void);



 __attribute__((gnu_inline)) inline int func_def_gnuin_redef (void) { return 0; }
 __attribute__((gnu_inline)) inline int func_def_gnuin_redef (void) { return 1; }



 inline int func_def_inline_redef (void) { return 0; }
 inline int func_def_inline_redef (void) { return 1; }



 __attribute__((gnu_inline)) inline int func_def_inline_after (void) { return 0; }
 inline int func_def_inline_after (void) { return 1; }



 inline int func_def_inline_before (void) { return 0; }
 __attribute__((gnu_inline)) inline int func_def_inline_before (void) { return 1; }



 int func_def_before (void) { return 0; }
 __attribute__((gnu_inline)) inline int func_def_before (void) { return 1; }



 static inline int func_decl_static_inline_before (void);
 __attribute__((gnu_inline)) inline int func_decl_static_inline_before (void) { return 0; }



 static int func_def_static_inline_after (void);
 __attribute__((gnu_inline)) inline int func_def_static_inline_after (void) { return 0; }
 static int func_def_static_inline_after (void);
 static inline int func_def_static_inline_after (void) { return 1; }
