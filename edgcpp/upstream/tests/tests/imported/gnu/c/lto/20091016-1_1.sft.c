//type: fp
//options: 
# 0 "./lto/20091016-1_1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20091016-1_1.c"
typedef struct VEC_constructor_elt_gcx { } VEC_constructor_elt_gc;
# 1 "./lto/20091016-1_a.h" 1
typedef union tree_node *tree;
union tree_node {
    tree * use;
    VEC_constructor_elt_gc *elts;
};
# 3 "./lto/20091016-1_1.c" 2
struct gcc_target {
    void (* builtin_vec_perm) (tree*);
};
extern struct gcc_target targetm;
void dwarf2out_begin_prologue (tree t)
{
    (*targetm.builtin_vec_perm) (&t);
}
struct die_arg_entry_struct {
    tree arg;
};
void *gt_pch_p_20VEC_die_arg_entry_gc (struct die_arg_entry_struct *vec)
{
    return &(vec->arg);
}
