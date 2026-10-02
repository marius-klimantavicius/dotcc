//type:ln
//options_all:--c++20 -tused -A
int foo();

namespace Q {
   struct C {
      template <unsigned N>
      struct B {
         typedef ::Q::C C;
         void foo();
      };
   };
}

template <unsigned N>
struct A : Q::C::B<N> {
   void bar() {
      Q::C::B<N> b;
      return b.C::/*template */B < N > ::foo();
   }
};

A<0> a;
void zip() { a.bar(); }

//cwg: 1835
//title: Dependent member lookup before <
//meeting: Virtual 11/20*
//edg_status: Passes
