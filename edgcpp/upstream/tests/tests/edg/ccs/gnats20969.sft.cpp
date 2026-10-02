//type:fp
//options:--gnu_version 30200:--microsoft_version 1920
//options_all:--c++17

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> { ~same(); };

struct S { unsigned long long x : 4, y : 32; int z; };

void f(S s) {
   auto [a, b, c] = s;

   same<decltype(+a), unsigned long long>();  // (A)
   same<decltype(+s.x), unsigned long long>();// (A)

   same<decltype(+b), unsigned long long>();  // (B)
   same<decltype(+s.y), unsigned long long>();// (B)
}
