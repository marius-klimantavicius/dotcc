//type: fp
//options: 
# 0 "./auto-init-uninit-pred-4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-pred-4.C"


# 1 "./uninit-pred-4.C" 1



int pop ();
int pop_first_bucket;

int my_pop ()
{
  int out;

  while (pop_first_bucket)
    if (pop_first_bucket && (out = pop()))
      return out;

  return 0;
}
# 4 "./auto-init-uninit-pred-4.C" 2
