//type: fp
//options: 
struct Foo {
  Foo () {}
  virtual void Func () /* count(-) { xfail *-*-* } */
  {}   /* count(-) { xfail *-*-* } */
};
