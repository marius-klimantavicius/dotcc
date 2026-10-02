//type:fp
//options_all:--c++17 -tused -A
   
   struct A { A(int = 0); }; 
   struct B : A { using A::A; }; 
   B b0(0); // #1 
   B b;     // #2 
