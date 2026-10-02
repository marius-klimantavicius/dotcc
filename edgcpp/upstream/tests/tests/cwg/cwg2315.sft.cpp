//type:fp
//options_all:--c++20 -tused -A
struct A {
  A();
  A(const A&);
};
union B {
  A a;
};

//cwg: 2315
//title: What is the “corresponding special member” of a variant member?
//meeting: Albuquerque 11/17
//edg_status: Passes
