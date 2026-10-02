//type:fn
//options_all:--c++17 -A
   auto a= 1 + 2;
   extern char &&a; // ok, redeclaration, could even be in a different TU 

//cwg: 2313
//title: Redeclaration of structured binding reference variables
//meeting: Albuquerque 11/17
//edg_status: Passes
