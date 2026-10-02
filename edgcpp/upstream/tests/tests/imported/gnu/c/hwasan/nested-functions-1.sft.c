//type: rn
//options: 
# 0 "./hwasan/nested-functions-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./hwasan/nested-functions-1.c"
# 15 "./hwasan/nested-functions-1.c"
# 1 "./hwasan/nested-functions-0.c" 1
# 15 "./hwasan/nested-functions-0.c"
__attribute__((noinline))
int *Ident(void *x) {
  return x;
}

int __attribute__ ((noinline))
intermediate (void (*f) (int, char),
       char num)
{
  if (num == 1)





    f (100, 100);
  else
    f (3, 100);

  return num % 3;
}

int* __attribute__ ((noinline))
nested_function (char num)
{
  int big_array[16];
  int other_array[16];
  void store (int index, char value)
    { big_array[index] = value; }
  return Ident(&other_array[intermediate (store, num)]);
}
# 16 "./hwasan/nested-functions-1.c" 2


int main ()
{
  nested_function (1);
  return 0;
}
