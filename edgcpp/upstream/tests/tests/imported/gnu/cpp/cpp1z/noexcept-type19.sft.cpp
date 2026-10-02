//type: fp
//options: --c++11
# 0 "./cpp1z/noexcept-type19.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp1z/noexcept-type19.C"


# 1 "./cpp1z/noexcept-type19.h" 1
       
# 2 "./cpp1z/noexcept-type19.h" 3


# 3 "./cpp1z/noexcept-type19.h" 3
typedef decltype(sizeof(0)) size_t;
extern "C" void *malloc (size_t) throw();
# 4 "./cpp1z/noexcept-type19.C" 2


# 5 "./cpp1z/noexcept-type19.C"
extern "C" void *malloc (size_t);

template<class T> void f(T*);

void *g(size_t);

int main()
{
  f<decltype(malloc)>(g);
}
