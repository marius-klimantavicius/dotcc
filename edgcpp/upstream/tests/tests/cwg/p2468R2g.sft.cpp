//type:fn
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
   bool c2 = C() == B();  // error, ambiguous between #2 found when searching C and reversed #2 found when searching B
   
