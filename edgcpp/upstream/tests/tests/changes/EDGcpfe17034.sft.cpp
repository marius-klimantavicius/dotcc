//type:fp
//options_all:--c++11
//remark:[4.12] SFINAE and C++11-style casts with braced operand
// 7/8/16   [EDGcpfe/17034,EDGcpfe/17052]
//
// SFINAE and C++11-style casts with braced operand
//
// When testing the validity of a C++11-style cast with a braced operand in a
// SFINAE context, the front end erroneously used a set of conversion criteria
// different from non-SFINAE contexts.  This could result in a candidate in an
// overload set being selected for a call (when it should have been discarded)
// and then triggering an error when trying to materialize the actual conversion.
//
// Here, substituting the cast "long{ T() }" with T = int* should disqualify
// candidate (1), but SFINAE processing used a conversion criterion that
// permitted the case.  Resolving the call (3) then produced an error explaining
// that an int* cannot be converted to type long.  This is now fixed (candidate
// (1) is discarded and (2) is selected).
template<typename T> decltype(long{ T() }) g(int);  // (1)
template<typename> int g(...);  // (2)
int main() {
  return g<int*>(42);  // (3)  Previously an error.  Now selected (2).
}
