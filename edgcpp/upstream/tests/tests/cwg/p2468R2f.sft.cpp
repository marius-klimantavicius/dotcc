//type:fn
//option::-A
//options_all:--c++20 -tused
struct A {};
   template<typename T> bool operator==(A, T);  // #1
   bool a1 = 0 == A();  // OK, calls reversed #1
   template<typename T> bool operator!=(A, T);
   bool a2 = 0 == A();  // error, #1 is not a rewrite target
   
