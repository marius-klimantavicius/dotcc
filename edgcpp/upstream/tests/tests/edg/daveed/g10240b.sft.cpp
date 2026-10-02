//remark:GNU asm declarations
//options:--gcc --gnu_version=30200;fn:--gcc --gnu_version=30300;fn:--g++ --gnu_version=30200;fn:--g++ --gnu_version=30300;fn

   void f() {
       int i = (__EDG_SIZE_TYPE__)"%[oops]";
       asm("
           : : "r"(i) );
   }
