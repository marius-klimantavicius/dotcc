//type: fp
//options: --c23
# 0 "./c23-if-decls-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-if-decls-2.c"





# 1 "./c23-if-decls-1.c" 1





void
g ()
{
  if (int i = 42);
  if (int i = 42; i > 10);
  if (int i, j; i = 42);
  switch (int i = 42);
  switch (int i = 42; i);
  switch (int i, j; i = 42);
}
# 7 "./c23-if-decls-2.c" 2
