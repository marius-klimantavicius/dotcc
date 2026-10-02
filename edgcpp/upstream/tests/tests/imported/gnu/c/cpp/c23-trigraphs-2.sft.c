//type: rp
//options: --c23
# 0 "./cpp/c23-trigraphs-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp/c23-trigraphs-2.c"




# 1 "./cpp/trigraphs.c" 1



extern void abort (void);
# 26 "./cpp/trigraphs.c"
  x ??'= 3;
  if (x != 6)
    abort ();

  if ((5 ??! 3) != 7)
    abort ();

  return 0;
??>
# 6 "./cpp/c23-trigraphs-2.c" 2
