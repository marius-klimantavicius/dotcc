//type: s
//options: 
# 0 "./gomp/pr41429.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./gomp/pr41429.C"




# 1 "./gomp/pr37189.C" 1




struct S
{
  S () {}
  S (S const &) {}
};

struct T
{
  S s;
};

void
bar (T &)
{
}

void
foo ()
{
  T t;
#pragma omp task
    bar (t);
}
# 6 "./gomp/pr41429.C" 2
