//source_files: pr66655_1.C
//type: rp
//options: 
# 0 "./pr66655.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr66655.C"



# 1 "./pr66655.h" 1
typedef int int32_t __attribute__((mode (__SI__)));

struct S
{
  static int32_t i;
  static void set (int32_t ii) { i = -ii; }
};
# 5 "./pr66655.C" 2

extern "C" void abort (void);



int32_t
g (void)
{
  return 0xabcd0123;
}

extern int32_t f (void);

int
main (void)
{
  S::set(0);
  if (f () != 0xabcd0123)
    abort ();
  return 0;
}
