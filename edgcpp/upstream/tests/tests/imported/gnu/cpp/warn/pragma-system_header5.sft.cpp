//type: fp
//options: 
# 0 "./warn/pragma-system_header5.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/pragma-system_header5.C"



# 1 "./warn/pragma-system_header5.h" 1
       
# 2 "./warn/pragma-system_header5.h" 3


# 3 "./warn/pragma-system_header5.h" 3
template <typename T> T g();
# 5 "./warn/pragma-system_header5.C" 2


# 6 "./warn/pragma-system_header5.C"
void f()
{
  g<const double>();
  g<volatile void>();
}
