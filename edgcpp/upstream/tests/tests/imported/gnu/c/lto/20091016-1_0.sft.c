//type: fp
//options: 
# 0 "./lto/20091016-1_0.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20091016-1_0.c"





typedef struct VEC_constructor_elt_gc { } VEC_constructor_elt_gc;
# 1 "./lto/20091016-1_a.h" 1
typedef union tree_node *tree;
union tree_node {
    tree * use;
    VEC_constructor_elt_gc *elts;
};
# 8 "./lto/20091016-1_0.c" 2
struct stmt_tree_s {
    tree x_cur_stmt_list;
};
void *add_stmt (struct stmt_tree_s *x)
{
  return &x->x_cur_stmt_list;
}
