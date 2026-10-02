//type: fn
//options: 
# 0 "./overload/error6a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./overload/error6a.C"


# 1 "./overload/error6.C" 1




template<class T> void f(T);
void f(int);

int main() {
  f<int>(0, 0);
}
# 4 "./overload/error6a.C" 2
