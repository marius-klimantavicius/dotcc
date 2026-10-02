//type: fp
//options: 
# 0 "./analyzer/torture/pr59037.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/pr59037.c"
# 1 "./analyzer/torture/../../../c-c++-common/pr59037.c" 1



typedef int v4si __attribute__ ((vector_size (16)));

int
main (int argc, char** argv)
{
  v4si x = {0,1,2,3};
  x = (v4si) {(x)[3], (x)[2], (x)[1], (x)[0]};
  return x[4];
}
# 2 "./analyzer/torture/pr59037.c" 2
