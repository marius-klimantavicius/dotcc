//type: fp
//options: --c23
# 0 "./c23-fordecl-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-fordecl-1.c"





# 1 "./c99-fordecl-2.c" 1





void
foo (void)
{


  int j = 0;
  for (int i = 1, bar (void); i <= 10; i++)

    j += i;

  for (static int i = 1; i <= 10; i++)

    j += i;

  for (extern int i; j <= 500; j++)

    j += 5;

  for (enum { FOO } i = FOO; i < 10; i++)

    j += i;

  for (enum BAR { FOO } i = FOO; i < 10; i++)


    j += i;
  for (typedef int T;;)
    ;
}
# 7 "./c23-fordecl-1.c" 2
