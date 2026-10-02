//remark:Core issue 2267: Initialization of temporary
//options:--c++20;fn

   struct A {} a; 
   struct B { explicit B(const A&); }; 

   struct D { D(); }; 
   struct C { explicit operator D(); } c; 

   const D &d3(c);     // #6, ok 
