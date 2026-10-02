//type: fn
//options: --c89
# 0 "./cpp/pr7263-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp/pr7263-2.c"



# 1 "./cpp/pr7263-2.h" 1
# 5 "./cpp/pr7263-2.c" 2
unsigned long long
bar ()
{
  return __extension__(0x1b27da572ef3cd86ULL);
}

unsigned long long
bar2 ()
{
  return 0x1b27da572ef3cd86ULL;
}


unsigned long long
bar3 ()
{
  return __extension__ (0x1b27da572ef3cd86ULL);
}

__extension__ unsigned long long
bar4 ()
{
  return 0x1b27da572ef3cd86ULL;
}
