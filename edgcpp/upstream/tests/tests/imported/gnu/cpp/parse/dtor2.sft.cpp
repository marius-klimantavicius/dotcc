//type: fn
//options: 
struct A {
  typedef A A2;
  ~A2(); // { dg-error "" }
};
