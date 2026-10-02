//type: fp
//options: --c17 --strict_gnu
# 0 "./permerror-fpermissive-nowarning.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./permerror-fpermissive-nowarning.c"





# 1 "./permerror-default.c" 1






void
implicit_function_declaration (void)
{
  f1 ();
}

extern implicit_int_1;
typedef implicit_int_2;
extern implicit_int_3 (void);
implicit_int_4 (i)

{
  (const) 0;
}

extern int missing_parameter_type (i);


int *
int_conversion_1 (int flag)
{
  void f2 (int *);
  flag ? "1" : 1;
  flag ? 1 : "1";
  f2 (flag);
  {
    int i1 = &flag;
    i1 = &flag;
  }
  return flag;
}

int
int_conversion_2 (int flag)
{
  void f3 (int);
  f3 (&flag);
  {
    int *i1 = flag;
    i1 = flag;
  }
  return &flag;
}

int *
incompatible_pointer_types (int flag)
{
  void f4 (int *);
  flag ? __builtin_abs : __builtin_labs;
  {
    int *p1 = __builtin_abs;
    p1 = __builtin_abs;
  }
  {
    int *p2 = incompatible_pointer_types;
    p2 = incompatible_pointer_types;
    {
      int *p3 = &p2;
      p3 = &p2;
    }
    f4 (&p2);
  }
  if (flag)
    return __builtin_abs;
  else
    return incompatible_pointer_types;
}

void
return_mismatch_1 (void)
{
  return 0;
}

int
return_mismatch_2 (void)
{
  return;
}
# 7 "./permerror-fpermissive-nowarning.c" 2
