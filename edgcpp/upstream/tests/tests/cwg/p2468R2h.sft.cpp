//type:fp
//option::-A
//options_all:--c++20 -tused
   
   struct B {
     bool operator==(const B&);  // #2
   };
   struct C : B {
     C();
     C(B);
     bool operator!=(const B&);  // #3
   };
   bool c1 = B() == C();  // OK, calls #2; reversed #2 is not a candidate because search for operator!= in C finds #3
   
   struct D {};
   template <typename T>
   bool operator==(D, T); // #4
   inline namespace N {
       template <typename T>
       bool operator!=(D, T); // #5
   }
   bool d1 = 0 == D(); // OK, calls reversed #4; #5 does not forbid #4 as a rewrite target

