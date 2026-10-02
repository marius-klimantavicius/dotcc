//type: fp
//options:  --c++11
# 0 "./vect/pr60559.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/pr60559.cc"





# 1 "./vect/pr60023.cc" 1





struct A { A (); ~A (); };

void
f1 (int *p, int *q, int *r) noexcept (true)
{
  int i;
  for (i = 0; i < 1024; i++)
    if (r[i])
      p[i] = q[i] + 1;
}

void
f2 (int *p, int *q, int *r)
{
  int i;
  for (i = 0; i < 1024; i++)
    if (r[i])
      p[i] = q[i] + 1;
}

void
f3 (int *p, int *q) noexcept (true)
{
  int i;
  for (i = 0; i < 1024; i++)
    p[i] = q[i] + 1;
}

void
f4 (int *p, int *q)
{
  int i;
  for (i = 0; i < 1024; i++)
    p[i] = q[i] + 1;
}

void
f5 (int *p, int *q, int *r) noexcept (true)
{
  int i;
  A a;
  for (i = 0; i < 1024; i++)
    if (r[i])
      p[i] = q[i] + 1;
}

void
f6 (int *p, int *q, int *r)
{
  int i;
  A a;
  for (i = 0; i < 1024; i++)
    if (r[i])
      p[i] = q[i] + 1;
}

void
f7 (int *p, int *q) noexcept (true)
{
  int i;
  A a;
  for (i = 0; i < 1024; i++)
    p[i] = q[i] + 1;
}

void
f8 (int *p, int *q)
{
  int i;
  A a;
  for (i = 0; i < 1024; i++)
    p[i] = q[i] + 1;
}
# 7 "./vect/pr60559.cc" 2
