//type: fp
//options: 
# 0 "./tree-ssa/builtin-snprintf-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/builtin-snprintf-4.c"
# 10 "./tree-ssa/builtin-snprintf-4.c"
# 1 "./tree-ssa/../range.h" 1
# 11 "./tree-ssa/../range.h"
typedef int int32_t;
typedef long int ptrdiff_t;
typedef long unsigned int size_t;

static inline ptrdiff_t signed_value (void)
{
  extern volatile ptrdiff_t signed_value_source;
  return signed_value_source;
}

static inline size_t unsigned_value (void)
{
  extern volatile size_t unsigned_value_source;
  return unsigned_value_source;
}

static inline ptrdiff_t signed_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return val < min || max < val ? min : val;
}

static inline ptrdiff_t signed_anti_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return min <= val && val <= max ? min == (-0x7fffffffffffffffL - 1) ? max + 1 : min - 1 : val;
}

static inline size_t unsigned_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return val < min || max < val ? min : val;
}

static inline size_t unsigned_anti_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return min <= val && val <= max ? min == 0 ? max + 1 : min - 1 : val;
}
# 11 "./tree-ssa/builtin-snprintf-4.c" 2

typedef __builtin_va_list va_list;

extern int snprintf (char*, size_t, const char*, ...);
extern int vsnprintf (char*, size_t, const char*, va_list);
# 47 "./tree-ssa/builtin-snprintf-4.c"
extern void sink (int, ...);
# 58 "./tree-ssa/builtin-snprintf-4.c"
static const size_t imax = 0x7fffffff;
static const size_t imaxp1 = imax + 1;





static const size_t dmax = 0x7fffffffffffffffL;

static const size_t dmaxp1 = dmax + 1;

static const size_t szmax = 0xffffffffffffffffUL;
static const size_t szmaxm1 = 0xffffffffffffffffUL - 1;


void test_size_cst (char **d)
{
  if ((0 > snprintf (*d++, imax, "%s", ""))) do { extern void call_in_true_branch_not_eliminated_on_line_75 (void); call_in_true_branch_not_eliminated_on_line_75(); } while (0); else (void)0;

  if (0 > snprintf (*d++, imaxp1, "%s", "")) do { extern void call_made_in_true_branch_on_line_77 (void); call_made_in_true_branch_on_line_77(); } while (0); else do { extern void call_made_in_false_branch_on_line_77 (void); call_made_in_false_branch_on_line_77(); } while (0);

  if (0 > snprintf (*d++, dmax, "%s", "")) do { extern void call_made_in_true_branch_on_line_79 (void); call_made_in_true_branch_on_line_79(); } while (0); else do { extern void call_made_in_false_branch_on_line_79 (void); call_made_in_false_branch_on_line_79(); } while (0);
  if (0 > snprintf (*d++, dmaxp1, "%s", "")) do { extern void call_made_in_true_branch_on_line_80 (void); call_made_in_true_branch_on_line_80(); } while (0); else do { extern void call_made_in_false_branch_on_line_80 (void); call_made_in_false_branch_on_line_80(); } while (0);
  if (0 > snprintf (*d++, szmaxm1, "%s", "")) do { extern void call_made_in_true_branch_on_line_81 (void); call_made_in_true_branch_on_line_81(); } while (0); else do { extern void call_made_in_false_branch_on_line_81 (void); call_made_in_false_branch_on_line_81(); } while (0);
  if (0 > snprintf (*d++, szmax, "%s", "")) do { extern void call_made_in_true_branch_on_line_82 (void); call_made_in_true_branch_on_line_82(); } while (0); else do { extern void call_made_in_false_branch_on_line_82 (void); call_made_in_false_branch_on_line_82(); } while (0);
}


void test_size_cst_va (char **d, va_list va)
{
  if ((0 > vsnprintf (*d++, imax, " ", va))) do { extern void call_in_true_branch_not_eliminated_on_line_88 (void); call_in_true_branch_not_eliminated_on_line_88(); } while (0); else (void)0;

  if (0 > vsnprintf (*d++, imaxp1, " ", va)) do { extern void call_made_in_true_branch_on_line_90 (void); call_made_in_true_branch_on_line_90(); } while (0); else do { extern void call_made_in_false_branch_on_line_90 (void); call_made_in_false_branch_on_line_90(); } while (0);

  if (0 > vsnprintf (*d++, dmax, " ", va)) do { extern void call_made_in_true_branch_on_line_92 (void); call_made_in_true_branch_on_line_92(); } while (0); else do { extern void call_made_in_false_branch_on_line_92 (void); call_made_in_false_branch_on_line_92(); } while (0);
  if (0 > vsnprintf (*d++, dmaxp1, " ", va)) do { extern void call_made_in_true_branch_on_line_93 (void); call_made_in_true_branch_on_line_93(); } while (0); else do { extern void call_made_in_false_branch_on_line_93 (void); call_made_in_false_branch_on_line_93(); } while (0);
  if (0 > vsnprintf (*d++, szmaxm1, " ", va)) do { extern void call_made_in_true_branch_on_line_94 (void); call_made_in_true_branch_on_line_94(); } while (0); else do { extern void call_made_in_false_branch_on_line_94 (void); call_made_in_false_branch_on_line_94(); } while (0);
  if (0 > vsnprintf (*d++, szmax, " ", va)) do { extern void call_made_in_true_branch_on_line_95 (void); call_made_in_true_branch_on_line_95(); } while (0); else do { extern void call_made_in_false_branch_on_line_95 (void); call_made_in_false_branch_on_line_95(); } while (0);
}


void test_size_range (char **d)
{
  size_t r = unsigned_range ((imax - 1), (imax));
  if ((0 > snprintf (*d++, r, "%s", ""))) do { extern void call_in_true_branch_not_eliminated_on_line_102 (void); call_in_true_branch_not_eliminated_on_line_102(); } while (0); else (void)0;

  r = unsigned_range ((imax), (imax + 1));
  if (0 > snprintf (*d++, r, "%s", "")) do { extern void call_made_in_true_branch_on_line_105 (void); call_made_in_true_branch_on_line_105(); } while (0); else do { extern void call_made_in_false_branch_on_line_105 (void); call_made_in_false_branch_on_line_105(); } while (0);

  r = unsigned_range ((imaxp1), (imaxp1 + 1));
  if (0 > snprintf (*d++, r, "%s", "")) do { extern void call_made_in_true_branch_on_line_108 (void); call_made_in_true_branch_on_line_108(); } while (0); else do { extern void call_made_in_false_branch_on_line_108 (void); call_made_in_false_branch_on_line_108(); } while (0);

  r = unsigned_range ((dmax), (dmaxp1));
  if (0 > snprintf (*d++, r, "%s", "")) do { extern void call_made_in_true_branch_on_line_111 (void); call_made_in_true_branch_on_line_111(); } while (0); else do { extern void call_made_in_false_branch_on_line_111 (void); call_made_in_false_branch_on_line_111(); } while (0);

  r = unsigned_range ((dmaxp1), (dmaxp1 + 1));
  if (0 > snprintf (*d++, r, "%s", "")) do { extern void call_made_in_true_branch_on_line_114 (void); call_made_in_true_branch_on_line_114(); } while (0); else do { extern void call_made_in_false_branch_on_line_114 (void); call_made_in_false_branch_on_line_114(); } while (0);

  r = unsigned_range ((szmaxm1), (szmax));
  if (0 > snprintf (*d++, r, "%s", "")) do { extern void call_made_in_true_branch_on_line_117 (void); call_made_in_true_branch_on_line_117(); } while (0); else do { extern void call_made_in_false_branch_on_line_117 (void); call_made_in_false_branch_on_line_117(); } while (0);
}


void test_size_range_va (char **d, va_list va)
{
  size_t r = unsigned_range ((imax - 1), (imax));
  if ((0 > vsnprintf (*d++, r, " ", va))) do { extern void call_in_true_branch_not_eliminated_on_line_124 (void); call_in_true_branch_not_eliminated_on_line_124(); } while (0); else (void)0;

  r = unsigned_range ((imax), (imax + 1));
  if (0 > vsnprintf (*d++, r, " ", va)) do { extern void call_made_in_true_branch_on_line_127 (void); call_made_in_true_branch_on_line_127(); } while (0); else do { extern void call_made_in_false_branch_on_line_127 (void); call_made_in_false_branch_on_line_127(); } while (0);

  r = unsigned_range ((imaxp1), (imaxp1 + 1));
  if (0 > vsnprintf (*d++, r, " ", va)) do { extern void call_made_in_true_branch_on_line_130 (void); call_made_in_true_branch_on_line_130(); } while (0); else do { extern void call_made_in_false_branch_on_line_130 (void); call_made_in_false_branch_on_line_130(); } while (0);

  r = unsigned_range ((dmax), (dmaxp1));
  if (0 > vsnprintf (*d++, r, " ", va)) do { extern void call_made_in_true_branch_on_line_133 (void); call_made_in_true_branch_on_line_133(); } while (0); else do { extern void call_made_in_false_branch_on_line_133 (void); call_made_in_false_branch_on_line_133(); } while (0);

  r = unsigned_range ((dmaxp1), (dmaxp1 + 1));
  if (0 > vsnprintf (*d++, r, " ", va)) do { extern void call_made_in_true_branch_on_line_136 (void); call_made_in_true_branch_on_line_136(); } while (0); else do { extern void call_made_in_false_branch_on_line_136 (void); call_made_in_false_branch_on_line_136(); } while (0);

  r = unsigned_range ((szmaxm1), (szmax));
  if (0 > vsnprintf (*d++, r, " ", va)) do { extern void call_made_in_true_branch_on_line_139 (void); call_made_in_true_branch_on_line_139(); } while (0); else do { extern void call_made_in_false_branch_on_line_139 (void); call_made_in_false_branch_on_line_139(); } while (0);
}


void test_size_varying (char **d, size_t n)
{
  if (0 > snprintf (*d++, n, "%s", "")) do { extern void call_made_in_true_branch_on_line_145 (void); call_made_in_true_branch_on_line_145(); } while (0); else do { extern void call_made_in_false_branch_on_line_145 (void); call_made_in_false_branch_on_line_145(); } while (0);

  n += 1;
  if (0 > snprintf (*d++, n, "%s", "")) do { extern void call_made_in_true_branch_on_line_148 (void); call_made_in_true_branch_on_line_148(); } while (0); else do { extern void call_made_in_false_branch_on_line_148 (void); call_made_in_false_branch_on_line_148(); } while (0);
}


void test_size_varying_va (char **d, size_t n, va_list va)
{
  if (0 > vsnprintf (*d++, n, " ", va)) do { extern void call_made_in_true_branch_on_line_154 (void); call_made_in_true_branch_on_line_154(); } while (0); else do { extern void call_made_in_false_branch_on_line_154 (void); call_made_in_false_branch_on_line_154(); } while (0);

  n += 1;
  if (0 > vsnprintf (*d++, n, " ", va)) do { extern void call_made_in_true_branch_on_line_157 (void); call_made_in_true_branch_on_line_157(); } while (0); else do { extern void call_made_in_false_branch_on_line_157 (void); call_made_in_false_branch_on_line_157(); } while (0);
}
