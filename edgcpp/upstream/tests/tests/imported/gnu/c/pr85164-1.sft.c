//type: fp
//options:  -w
/* { dg-options "-fpermissive -O2 -w" } */
a[];
b;
c() {
  unsigned long d;
  b = a[d - 1 >> 3];
}
