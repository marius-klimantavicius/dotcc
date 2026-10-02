//type: fp
//options: 
# 0 "./analyzer/conditionals-trans.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/conditionals-trans.c"

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
# 3 "./analyzer/conditionals-trans.c" 2

void test (int i, int j)
{
  if (i > 4)
    {
      __analyzer_eval (i > 4);
      __analyzer_eval (i <= 4);
      __analyzer_eval (i > 3);

      __analyzer_eval (i > 5);
      __analyzer_eval (i != 3);

      __analyzer_eval (i == 3);

      __analyzer_eval (i != 4);
      __analyzer_eval (i == 4);
      __analyzer_eval (i == 5);
      __analyzer_eval (i != 5);
      __analyzer_eval (i < 5);
      __analyzer_eval (i <= 5);


      if (j < i)
 {
   __analyzer_eval (j < i);
   __analyzer_eval (j <= 4);
 }
      else
 {
   __analyzer_eval (j >= i);
   __analyzer_eval (j > 4);
 }
    }
  else
    {
      __analyzer_eval (i > 4);
      __analyzer_eval (i <= 4);
      __analyzer_eval (i > 3);

      __analyzer_eval (i > 5);
      __analyzer_eval (i != 3);

      __analyzer_eval (i == 3);

      __analyzer_eval (i != 4);
      __analyzer_eval (i == 4);
      __analyzer_eval (i == 5);
      __analyzer_eval (i != 5);
      __analyzer_eval (i < 5);
      __analyzer_eval (i <= 5);
    }
}

void test_2 (int i, int j, int k)
{
  if (i >= j)
    {
      __analyzer_eval (i == k);
      if (j >= k)
 {
   __analyzer_eval (i >= k);
   __analyzer_eval (i == k);
   if (k >= i)
     __analyzer_eval (i == k);
 }
    }
}

void test_3 (int flag, unsigned int i)
{
  if (!flag) {
    return;
  }

  __analyzer_eval (flag);

  if (i>0) {
    __analyzer_eval (i > 0);
    __analyzer_eval (flag);
  } else {
    __analyzer_eval (i <= 0);
    __analyzer_eval (flag);
  }

  __analyzer_eval (flag);
}

void test_range_int_gt_lt (int i)
{
  if (i > 3)
    if (i < 5)
      __analyzer_eval (i == 4);
}

void test_range_float_gt_lt (float f)
{
  if (f > 3)
    if (f < 5)
      __analyzer_eval (f == 4);
}

void test_range_int_ge_lt (int i)
{
  if (i >= 4)
    if (i < 5)
      __analyzer_eval (i == 4);
}

void test_range_float_ge_lt (float f)
{
  if (f >= 4)
    if (f < 5)
      __analyzer_eval (f == 4);
}

void test_range_int_gt_le (int i)
{
  if (i > 3)
    if (i <= 4)
      __analyzer_eval (i == 4);
}

void test_range_float_gt_le (float f)
{
  if (f > 3)
    if (f <= 4)
      __analyzer_eval (f == 4);
}

void test_range_int_ge_le (int i)
{
  if (i >= 4)
    if (i <= 4)
      __analyzer_eval (i == 4);
}

void test_range_float_ge_le (float f)
{
  if (f >= 4)
    if (f <= 4)
      __analyzer_eval (f == 4);

}

void test_float_selfcmp (float f)
{
  __analyzer_eval (f == f);
  __analyzer_eval (f != f);
}
