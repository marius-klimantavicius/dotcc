//type:fp
//options_all:--c++11
//remark:[6.0] Spurious error on deleted copy constructor with conditional comma expressions
// 11/20/19 [EDGcpfe/21979]
//
// Spurious error on deleted copy constructor with conditional comma expressions
//
// When the second and third operands to the conditional operator were comma
// expressions, the front end was failing to recognize when these comma
// expressions were movable.  This led to an incorrectly called copy constructor.
//
// This is now fixed.
struct S
{
  S(int);
  S(const S&) = delete;
  S(S&&);
};
void f()
{
  1 ? (1,S(1)) : (1,S(2)); // Previously emitted an error due to the deleted
                           // copy ctor being called, now accepted
}
