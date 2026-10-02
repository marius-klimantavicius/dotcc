//type: rp
//options:  mv12-aux.cc
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
# 1 "./ext/mv12.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./ext/mv12.C"
# 9 "./ext/mv12.C"
# 1 "./ext/mv12.h" 1


int foo () __attribute__ ((target ("default")));
int foo () __attribute__ ((target ("sse4.2")));
# 10 "./ext/mv12.C" 2

int main ()
{
  if (__builtin_cpu_supports ("sse4.2"))
    return foo () - 1;
  return foo ();
}

__attribute__ ((target ("default")))
int foo ()
{
  return 0;
}
