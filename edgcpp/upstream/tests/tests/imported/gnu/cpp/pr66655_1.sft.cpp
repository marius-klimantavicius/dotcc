//type: fp
//options: 
# 0 "./pr66655_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr66655_1.C"
# 1 "./pr66655.h" 1
typedef int int32_t __attribute__((mode (__SI__)));

struct S
{
  static int32_t i;
  static void set (int32_t ii) { i = -ii; }
};
# 2 "./pr66655_1.C" 2

extern int32_t g (void);

int32_t S::i;

int32_t
f (void)
{
  int32_t ret = g ();

  S::set (ret);
  return ret;
}
