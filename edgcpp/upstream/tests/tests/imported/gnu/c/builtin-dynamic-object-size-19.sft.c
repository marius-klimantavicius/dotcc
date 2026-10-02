//type: fp
//options: 
# 0 "./builtin-dynamic-object-size-19.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-object-size-19.c"





# 1 "./builtin-object-size-18.c" 1






typedef long unsigned int size_t;

size_t
foo (const char *p, size_t s, size_t t)
{
  char buf[64];
  char *q = __builtin___stpncpy_chk (buf, p, s, t);
  return __builtin_dynamic_object_size (q, 2);
}
# 7 "./builtin-dynamic-object-size-19.c" 2

typedef long unsigned int size_t;
# 28 "./builtin-dynamic-object-size-19.c"
void sink (void*);
# 40 "./builtin-dynamic-object-size-19.c"
__attribute__ ((alloc_size (1))) void* (*alloc_1_x)(size_t, size_t);
__attribute__ ((alloc_size (2))) void* (*alloc_x_2)(size_t, size_t);



typedef __attribute__ ((alloc_size (1, 2))) void* (alloc_1_2_t)(size_t, size_t);

void test_alloc_ptr (alloc_1_2_t *alloc_1_2)
{
  do { void *p = alloc_1_x (0, 0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_49 (void); call_in_true_branch_not_eliminated_on_line_49(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_49 (void); call_in_true_branch_not_eliminated_on_line_49(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_49 (void); call_in_true_branch_not_eliminated_on_line_49(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_49 (void); call_in_true_branch_not_eliminated_on_line_49(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_1_x (1, 0); sink (p); if (!(1 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_50 (void); call_in_true_branch_not_eliminated_on_line_50(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_50 (void); call_in_true_branch_not_eliminated_on_line_50(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_50 (void); call_in_true_branch_not_eliminated_on_line_50(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_50 (void); call_in_true_branch_not_eliminated_on_line_50(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_1_x (3, 0); sink (p); if (!(3 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_51 (void); call_in_true_branch_not_eliminated_on_line_51(); } while (0); else (void)0; if (!(3 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_51 (void); call_in_true_branch_not_eliminated_on_line_51(); } while (0); else (void)0; if (!(3 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_51 (void); call_in_true_branch_not_eliminated_on_line_51(); } while (0); else (void)0; if (!(3 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_51 (void); call_in_true_branch_not_eliminated_on_line_51(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_1_x (9, 5); sink (p); if (!(9 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_52 (void); call_in_true_branch_not_eliminated_on_line_52(); } while (0); else (void)0; if (!(9 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_52 (void); call_in_true_branch_not_eliminated_on_line_52(); } while (0); else (void)0; if (!(9 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_52 (void); call_in_true_branch_not_eliminated_on_line_52(); } while (0); else (void)0; if (!(9 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_52 (void); call_in_true_branch_not_eliminated_on_line_52(); } while (0); else (void)0; } while (0);

  do { void *p = alloc_x_2 (0, 0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_54 (void); call_in_true_branch_not_eliminated_on_line_54(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_54 (void); call_in_true_branch_not_eliminated_on_line_54(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_54 (void); call_in_true_branch_not_eliminated_on_line_54(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_54 (void); call_in_true_branch_not_eliminated_on_line_54(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_x_2 (1, 0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_55 (void); call_in_true_branch_not_eliminated_on_line_55(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_55 (void); call_in_true_branch_not_eliminated_on_line_55(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_55 (void); call_in_true_branch_not_eliminated_on_line_55(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_55 (void); call_in_true_branch_not_eliminated_on_line_55(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_x_2 (0, 1); sink (p); if (!(1 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_56 (void); call_in_true_branch_not_eliminated_on_line_56(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_56 (void); call_in_true_branch_not_eliminated_on_line_56(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_56 (void); call_in_true_branch_not_eliminated_on_line_56(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_56 (void); call_in_true_branch_not_eliminated_on_line_56(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_x_2 (9, 5); sink (p); if (!(5 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_57 (void); call_in_true_branch_not_eliminated_on_line_57(); } while (0); else (void)0; if (!(5 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_57 (void); call_in_true_branch_not_eliminated_on_line_57(); } while (0); else (void)0; if (!(5 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_57 (void); call_in_true_branch_not_eliminated_on_line_57(); } while (0); else (void)0; if (!(5 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_57 (void); call_in_true_branch_not_eliminated_on_line_57(); } while (0); else (void)0; } while (0);

  do { void *p = alloc_1_2 (0, 0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_59 (void); call_in_true_branch_not_eliminated_on_line_59(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_59 (void); call_in_true_branch_not_eliminated_on_line_59(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_59 (void); call_in_true_branch_not_eliminated_on_line_59(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_59 (void); call_in_true_branch_not_eliminated_on_line_59(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_1_2 (1, 0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_60 (void); call_in_true_branch_not_eliminated_on_line_60(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_60 (void); call_in_true_branch_not_eliminated_on_line_60(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_60 (void); call_in_true_branch_not_eliminated_on_line_60(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_60 (void); call_in_true_branch_not_eliminated_on_line_60(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_1_2 (0, 1); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_61 (void); call_in_true_branch_not_eliminated_on_line_61(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_61 (void); call_in_true_branch_not_eliminated_on_line_61(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_61 (void); call_in_true_branch_not_eliminated_on_line_61(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_61 (void); call_in_true_branch_not_eliminated_on_line_61(); } while (0); else (void)0; } while (0);
  do { void *p = alloc_1_2 (9, 5); sink (p); if (!(45 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_62 (void); call_in_true_branch_not_eliminated_on_line_62(); } while (0); else (void)0; if (!(45 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_62 (void); call_in_true_branch_not_eliminated_on_line_62(); } while (0); else (void)0; if (!(45 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_62 (void); call_in_true_branch_not_eliminated_on_line_62(); } while (0); else (void)0; if (!(45 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_62 (void); call_in_true_branch_not_eliminated_on_line_62(); } while (0); else (void)0; } while (0);
}





typedef void *(allocfn_1) (size_t);
typedef void *(allocfn_1_2) (size_t, size_t);

static inline void *
call_alloc (allocfn_1 *fn1, allocfn_1_2 *fn2, size_t n1, size_t n2)
{
  return fn1 ? fn1 (n1) : fn2 (n1, n2);
}

static inline void *
call_malloc (size_t n)
{
  return call_alloc (__builtin_malloc, 0, n, 0);
}

static inline void *
call_calloc (size_t n1, size_t n2)
{
  return call_alloc (0, __builtin_calloc, n1, n2);
}

void test_builtin_ptr (void)
{
  do { void *p = call_malloc (0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_92 (void); call_in_true_branch_not_eliminated_on_line_92(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_92 (void); call_in_true_branch_not_eliminated_on_line_92(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_92 (void); call_in_true_branch_not_eliminated_on_line_92(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_92 (void); call_in_true_branch_not_eliminated_on_line_92(); } while (0); else (void)0; } while (0);
  do { void *p = call_malloc (1); sink (p); if (!(1 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_93 (void); call_in_true_branch_not_eliminated_on_line_93(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_93 (void); call_in_true_branch_not_eliminated_on_line_93(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_93 (void); call_in_true_branch_not_eliminated_on_line_93(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_93 (void); call_in_true_branch_not_eliminated_on_line_93(); } while (0); else (void)0; } while (0);
  do { void *p = call_malloc (9); sink (p); if (!(9 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_94 (void); call_in_true_branch_not_eliminated_on_line_94(); } while (0); else (void)0; if (!(9 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_94 (void); call_in_true_branch_not_eliminated_on_line_94(); } while (0); else (void)0; if (!(9 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_94 (void); call_in_true_branch_not_eliminated_on_line_94(); } while (0); else (void)0; if (!(9 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_94 (void); call_in_true_branch_not_eliminated_on_line_94(); } while (0); else (void)0; } while (0);

  do { void *p = call_calloc (0, 0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_96 (void); call_in_true_branch_not_eliminated_on_line_96(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_96 (void); call_in_true_branch_not_eliminated_on_line_96(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_96 (void); call_in_true_branch_not_eliminated_on_line_96(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_96 (void); call_in_true_branch_not_eliminated_on_line_96(); } while (0); else (void)0; } while (0);
  do { void *p = call_calloc (0, 1); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_97 (void); call_in_true_branch_not_eliminated_on_line_97(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_97 (void); call_in_true_branch_not_eliminated_on_line_97(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_97 (void); call_in_true_branch_not_eliminated_on_line_97(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_97 (void); call_in_true_branch_not_eliminated_on_line_97(); } while (0); else (void)0; } while (0);
  do { void *p = call_calloc (1, 0); sink (p); if (!(0 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_98 (void); call_in_true_branch_not_eliminated_on_line_98(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_98 (void); call_in_true_branch_not_eliminated_on_line_98(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_98 (void); call_in_true_branch_not_eliminated_on_line_98(); } while (0); else (void)0; if (!(0 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_98 (void); call_in_true_branch_not_eliminated_on_line_98(); } while (0); else (void)0; } while (0);
  do { void *p = call_calloc (1, 1); sink (p); if (!(1 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_99 (void); call_in_true_branch_not_eliminated_on_line_99(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_99 (void); call_in_true_branch_not_eliminated_on_line_99(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_99 (void); call_in_true_branch_not_eliminated_on_line_99(); } while (0); else (void)0; if (!(1 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_99 (void); call_in_true_branch_not_eliminated_on_line_99(); } while (0); else (void)0; } while (0);
  do { void *p = call_calloc (1, 3); sink (p); if (!(3 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_100 (void); call_in_true_branch_not_eliminated_on_line_100(); } while (0); else (void)0; if (!(3 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_100 (void); call_in_true_branch_not_eliminated_on_line_100(); } while (0); else (void)0; if (!(3 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_100 (void); call_in_true_branch_not_eliminated_on_line_100(); } while (0); else (void)0; if (!(3 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_100 (void); call_in_true_branch_not_eliminated_on_line_100(); } while (0); else (void)0; } while (0);
  do { void *p = call_calloc (2, 3); sink (p); if (!(6 == __builtin_dynamic_object_size (p, 0))) do { extern void call_in_true_branch_not_eliminated_on_line_101 (void); call_in_true_branch_not_eliminated_on_line_101(); } while (0); else (void)0; if (!(6 == __builtin_dynamic_object_size (p, 1))) do { extern void call_in_true_branch_not_eliminated_on_line_101 (void); call_in_true_branch_not_eliminated_on_line_101(); } while (0); else (void)0; if (!(6 == __builtin_dynamic_object_size (p, 2))) do { extern void call_in_true_branch_not_eliminated_on_line_101 (void); call_in_true_branch_not_eliminated_on_line_101(); } while (0); else (void)0; if (!(6 == __builtin_dynamic_object_size (p, 3))) do { extern void call_in_true_branch_not_eliminated_on_line_101 (void); call_in_true_branch_not_eliminated_on_line_101(); } while (0); else (void)0; } while (0);
}
