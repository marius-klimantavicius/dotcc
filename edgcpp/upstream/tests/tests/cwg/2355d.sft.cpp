//type:fn
//options_all:--c++20 -tused
   template<bool> struct A { };
   template<bool B> void f(void (*)(A<B>) noexcept(B));
   void g(A<false>) noexcept;
   void h() {
     f(g);    // ill-formed; previously well-formed.
   }
