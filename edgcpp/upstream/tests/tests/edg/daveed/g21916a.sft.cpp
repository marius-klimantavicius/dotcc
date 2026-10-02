//remark:Operator template substitution
//options:--c++11;fp

   struct A { };
   template <typename T> struct B {
        using a = A*;
        operator const T&();
   };
   struct D : public A { };
   template <typename T> bool operator==(const T& a, const A* b);
   bool g(D* x, B<A*> y) { return x == y; }

