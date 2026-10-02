//type: fp
//options: 
# 0 "./warn/pragma-system_header2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/pragma-system_header2.C"



# 1 "./warn/pragma-system_header2.h" 1
template <typename T>
  int g() { double d = 0.1; return d; }

template <typename T>
  T h() { double d = 0.1; return d; }
# 5 "./warn/pragma-system_header2.C" 2


void f()
{
  g<int>();
  h<int>();
}
