//type: rp
//options: 
# 0 "./tree-ssa/pr96779-disabled.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr96779-disabled.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 7 "./tree-ssa/pr96779-disabled.c" 2

bool __attribute__ ((noipa)) f_func(int a)
{
    return -a == a;
}

bool __attribute__ ((noipa)) g_func(unsigned int a)
{
    return -a == a;
}

bool __attribute__ ((noipa)) h_func(short a)
{
    return -a == a;
}

bool __attribute__ ((noipa)) k_func(long a)
{
    return -a == a;
}

int
main (void)
{

  if (f_func (71856034))
    {
      __builtin_abort ();
    }
  if (g_func (71856034))
    {
      __builtin_abort ();
    }
  if (h_func (1744))
    {
      __builtin_abort ();
    }
  if (k_func (68268386))
    {
      __builtin_abort ();
    }
  if (f_func (-112237))
    {
      __builtin_abort ();
    }
  if (g_func (-787116))
    {
      __builtin_abort ();
    }
  if (h_func (-863))
    {
      __builtin_abort ();
    }
  if (k_func (-787116))
    {
      __builtin_abort ();
    }
  if (!f_func (0))
    {
      __builtin_abort ();
    }
  if (!g_func (0))
    {
      __builtin_abort ();
    }
  if (!h_func (0))
    {
      __builtin_abort ();
    }
  if (!k_func (0))
    {
      __builtin_abort ();
    }

  return 0;
}
