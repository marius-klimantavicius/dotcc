//type:fp
//options_all:--c++20 -A -tused
  struct A {}; 
  struct B : A {}; 
  using T = const B; 
  A a = true ? A() : T();

//cwg: 2321
//title: Conditional operator and cv-qualified class prvalues
//meeting: Rapperswil 6/18
//edg_status: Passes
