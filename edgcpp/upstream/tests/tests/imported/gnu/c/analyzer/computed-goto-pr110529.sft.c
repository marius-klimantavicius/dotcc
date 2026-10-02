//type: fp
//options: 
# 0 "./analyzer/computed-goto-pr110529.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/computed-goto-pr110529.c"


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
# 4 "./analyzer/computed-goto-pr110529.c" 2

void foo(int pc) {
    int *arr[2] = {&&x, &&y};
    int var = 0;
    __analyzer_dump_path ();

    goto *arr[pc];

x:
    __analyzer_dump_path ();
    __analyzer_eval (pc == 0);

    arr[0] = (void *)0;
    *arr[0] = 10086;
    return;
y:
    __analyzer_dump_path ();
    __analyzer_eval (pc == 1);


    return;
}

int main() { foo(0); }
