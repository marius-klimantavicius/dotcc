//remark:Core issue 2267: Initialization of temporary
//options:--c++20;fn

   struct A {} a; 
   struct B { explicit B(const A&); }; 

   struct D { D(); }; 
   struct C { explicit operator D(); } c; 

   B b1(a);            // #1, ok 
   const B &b2{a};     // #2. ok 
   const B &b3(a);     // #3, error 

   D d1(c);            // #4, ok 
   const D &d2{c};     // #5, ok 
   const D &d3(c);     // #6, ok 
