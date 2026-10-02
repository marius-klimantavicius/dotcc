//type: fp
//options: 
# 0 "./warn/pragma-system_header1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/pragma-system_header1.C"



# 1 "./warn/pragma-system_header1.h" 1
       
# 2 "./warn/pragma-system_header1.h" 3


# 3 "./warn/pragma-system_header1.h" 3
template <typename T>
  int g() { double d = 0.1; return d; }

template <typename T>
  T h() { double d = 0.1; return d; }
# 5 "./warn/pragma-system_header1.C" 2


# 6 "./warn/pragma-system_header1.C"
void f()
{
  g<int>();
  h<int>();
}
