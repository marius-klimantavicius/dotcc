//type:fp
//options::--gnu_version 70000
//options_all:--c++17

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> { ~same(); };

struct S { unsigned long long x : 4, y : 32; int z; };

void f(S s) {
   auto [a, b, c] = s;

   same<decltype(+a), int>();           // (A)
   same<decltype(+s.x), int>();         // (A1)
   
   same<decltype(+b), unsigned int>();  // (B)
   same<decltype(+s.y), unsigned int>();// (B1)
}
