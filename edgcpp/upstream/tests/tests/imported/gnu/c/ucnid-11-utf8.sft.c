//type: rp
//options: --c99
# 0 "./ucnid-11-utf8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ucnid-11-utf8.c"






# 1 "./ucnid-3-utf8.c" 1




void abort (void);

int \U000000c0 = 1;
int \U000000c1 = 2;
int \U000000c2 = 3;
int wh\U000000ff = 4;
int a\U000000c4b\U00000441\U000003b4e = 5;

int main (void)
{

  if (\U000000c0 != 1)
    abort ();
  if (\U000000c1 != 2)
    abort ();
  if (\U000000c2 != 3)
    abort ();
  if (wh\U000000ff != 4)
    abort ();
  if (a\U000000c4b\U00000441\U000003b4e != 5)
    abort ();

  return 0;
}
# 8 "./ucnid-11-utf8.c" 2
