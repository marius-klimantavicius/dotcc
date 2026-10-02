//type:fp
//remark:[4.12] Cast type vs. expression disambiguation
// 6/10/16  [EDGcpfe/17010]
//
// Cast type vs. expression disambiguation
//
// An expression like
//
// where T is a type name was previously erroneously treated as an attempt to
// cast "()" to type "T ()" (i.e., a function type taking no arguments and
// returning type T).  Now, it is instead treated as the invocation of a call
// with no arguments on the value-initialization of an object of type T.
//
// Note that the following similar arrangement is still invalid (as mandated by
// the standard):
//
// Changes were also made to expressions of the form "(T())++" and "(T())--"
// (which are now accepted if the ++/-- is not followed by another expression).
struct S {
  int operator()();
};
int r = (S())();  // Previously a syntax error.  Now okay.
