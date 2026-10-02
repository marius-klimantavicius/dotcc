//remark:Operator template substitution
//options:--c++11;fp


   template<typename T> struct X { operator T const&(); };
   struct B {};
   struct D : public B {};
   template<typename T> bool operator==(T const&, B const*);
   bool g(D *x, X<B*> y) { return x == y; }

