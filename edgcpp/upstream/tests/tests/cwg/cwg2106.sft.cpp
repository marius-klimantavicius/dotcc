//type:fn
//options_all:--c++17 -tused -A
//
   template<class T> struct A {
     static T t;
   };
   typedef int function();
   A<function> a;   // ill-formed: would declare A<function>::t
                    // as a static member function

//cwg: 2106
//title: Unclear restrictions on use of function-type template arguments
//meeting: Jacksonville 2/16
//edg_status: Passes
