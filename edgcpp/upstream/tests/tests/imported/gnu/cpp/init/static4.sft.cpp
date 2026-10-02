//type: fn
//options: 
// PR c++/51461

struct A
{
  static const A a = 0;		// { dg-error "incomplete|non-integral" }
};
