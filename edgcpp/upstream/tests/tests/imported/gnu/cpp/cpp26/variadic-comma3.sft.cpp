//type: fp
//options: --c++11
# 0 "./cpp26/variadic-comma3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp26/variadic-comma3.C"




# 1 "./cpp26/variadic-comma1.C" 1



void f1 (int...);




template <typename ...T>
void f4 (T......);
template <typename ...T>
void f5 (T...);
template <typename ...T>
void f6 (T..., int...);
void
f7 (char...)
{
}
# 6 "./cpp26/variadic-comma3.C" 2
