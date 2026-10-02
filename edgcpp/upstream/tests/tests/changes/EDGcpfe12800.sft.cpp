//type:fp
//remark:[4.5] Class modifiers on definition of classes nested in class templates
// 3/28/12  [EDGcpfe/12800]
//
// Class modifiers on definition of classes nested in class templates
//
// In Microsoft C++ mode, the front end previously issued spurious errors on a
// definition of a class nested in a class template if that nested class included
// a modifier "sealed" or "abstract" (the error occurred during the instantiation
// of the class template).
//
// This is now fixed.
template<class T> struct S {
  struct N sealed {};  // "sealed" is a class modifier.
};
S<int> s;  // This previously triggered an error for S<int>::N.
