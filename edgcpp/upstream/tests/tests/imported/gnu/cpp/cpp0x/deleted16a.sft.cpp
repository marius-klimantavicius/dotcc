//type: fn
//options: --c++11
# 0 "./cpp0x/deleted16a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/deleted16a.C"



# 1 "./cpp0x/deleted16.C" 1





void f(int) = delete;
void f(...);
void f(int, int);



void g(int) = delete;
template<class T> void g(T);



template<class T> void h(T, T) = delete;
void h(int*, int) = delete;

int main() {
  f(0);
  g(0);
  h(1, 1);

}
# 5 "./cpp0x/deleted16a.C" 2
