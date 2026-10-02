//type: fp
//options: 
# 0 "./pr90773-1c.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr90773-1c.C"




# 1 "./pr90773-1a.C" 1




# 1 "./pr90773-1.h" 1
class fixed_wide_int_storage {
public:
  long val[10];
  int len;
  fixed_wide_int_storage ()
    {
      len = sizeof (val) / sizeof (val[0]);
      for (int i = 0; i < len; i++)
 val[i] = i;
    }
};

extern void foo (fixed_wide_int_storage);
extern int record_increment(void);
# 6 "./pr90773-1a.C" 2

int
record_increment(void)
{
  fixed_wide_int_storage x;
  foo (x);
  return 0;
}
# 6 "./pr90773-1c.C" 2
