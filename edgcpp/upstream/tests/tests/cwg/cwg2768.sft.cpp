//type:fn
//options_all:--c++23
   enum class E {E1};

   void f() {
     E e;
     e = E{0}; // #1
     e = {0};  // #2
   }

//cwg: 2768
//title: Assignment to enumeration variable with a braced-init-list
//meeting: Kona 11/23
//edg_status: Passes
