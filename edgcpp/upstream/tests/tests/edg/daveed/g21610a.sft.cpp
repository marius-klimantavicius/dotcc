//remark:No-effect warnings
//options:--gcc --diag_error=174;fp

extern void __assert_fail (const char*);

/* The first occurrence of EXPR is not evaluated due to the sizeof,
   but will trigger any pedantic warnings masked by the __extension__
   for the second occurrence.  The ternary operator is required to
   support function pointers and bit fields in this context, and to
   suppress the evaluation of variable length arrays.  */
#define assert(expr) \
  ((void) sizeof ((expr) ? 1 : 0), __extension__ ({ \
      if (expr) \
        ; /* empty */ \
      else \
        __assert_fail (#expr); \
    }))

void foo (void)
{
  assert (3+4);
}
