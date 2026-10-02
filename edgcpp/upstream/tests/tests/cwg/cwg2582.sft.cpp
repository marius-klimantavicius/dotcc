//type:fn
//options_all:--c++20 -tused -A
  typedef int T;
  struct A {
   struct B {
    static T t;
   };
   typedef float T; // IFNDR?
  };

//cwg: 2582
//title: Differing member lookup from nested classes
//meeting: Virtual 7/22
//edg_status: N/A (IFNDR)
