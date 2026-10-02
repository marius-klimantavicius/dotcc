//options_all:--c++23 -A
 static int x = 1;
  template<auto y = x> void f() {}
/* ill-formed NDR */

//cwg: 2746
//title: Checking of default template arguments
//meeting: Tokyo 3/24
//edg_status: Passes
