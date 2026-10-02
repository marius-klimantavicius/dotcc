//type:fp
//options_all:--g++ --c++11 -DCPP11
//remark:[4.14] Tag names and declarations in condition scopes
// 5/3/17   [EDGcpfe/18179]
//
// Tag names and declarations in condition scopes
//
// A name declared in a condition scope cannot also be declared in the scope that
// it immediately encloses.  The front end was previously enforcing that rule
// strictly in all modes (as indicated by the C++11 standard).  Now, in nonstrict
// modes, tag names can coexist with non-tag names.
void g() {
  if (int N = 3) {
    struct N {} n;  // Previously an error.  Now okay in nonstrict modes.
  }
}
