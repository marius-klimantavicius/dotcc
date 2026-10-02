//type:fp
//options_all:--c++17 -tused -A
    struct B { int i; };         // standard-layout class
   struct C : B { };            // standard-layout class
   struct D : C { };            // standard-layout class
   struct E : D { char : 4; };  // not a standard-layout class

   struct Q {};
   struct S : Q { };
   struct T : Q { };
   struct U : S, T { };         // not a standard-layout class

   int main()
   {
       static_assert(__is_standard_layout(B));
       static_assert(__is_standard_layout(C));
       static_assert(__is_standard_layout(D));
       static_assert(!__is_standard_layout(E));
     static_assert(!__is_standard_layout(U));

   }

//cwg: 1813
//title: Direct vs indirect bases in standard-layout classes
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
//fixed_in: 6.7
