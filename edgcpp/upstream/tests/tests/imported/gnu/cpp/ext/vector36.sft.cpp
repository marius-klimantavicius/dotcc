//type: fp
//options: 
# 0 "./ext/vector36.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/vector36.C"





# 1 "./ext/vector27.C" 1


typedef int veci __attribute__ ((vector_size (4 * sizeof (int))));
typedef float vecf __attribute__ ((vector_size (4 * sizeof (float))));

void f (veci *a, veci *b, int c)
{
  *a = !*a || *b < ++c;
}
void g (vecf *a, vecf *b)
{
  *a = (*a < 1 && !(*b > 2)) ? *a + *b : 3;
}
# 7 "./ext/vector36.C" 2
