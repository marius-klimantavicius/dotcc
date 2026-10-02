//type:fn
//options_all:--c++14 -tused -A
  // _CXX11 - Implements core 1805 - in C++14 status 2015
   struct A {
       A(const char* a) { }
       operator const char*() { return "a"; }
   };
   A a ("a");
   const char* b(bool c) {
       return c ? a : ""; // error - after array-to-ptr, ambiguous
   }
   int mymain() {
       return 0;
   }

//cwg: 1805
//title: Conversions of array operands in conditional-expressions
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/17601
