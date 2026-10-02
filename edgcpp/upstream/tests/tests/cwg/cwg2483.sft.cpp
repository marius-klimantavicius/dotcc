//options_all:--c++20 -A -tused
   extern "C" {
     struct A {
       static void f();
       constexpr static void (*p)()=f; 
     };
   }

//cwg: 2483
//title: Language linkage of static member functions
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/26055
