//type:fp
//options_all:--c++11 --g++ --diag_error=811
//remark:[6.5] Variables of a class type with a defaulted default constructor
// 4/5/23   [EDGcpfe/21469,EDGcpfe/24285,EDGcpfe/26180]
//
// Variables of a class type with a defaulted default constructor
//
// Previously, this elicited an error because S has no user-provided constructor
// (or a warning in nonstrict modes).  However, the language allows such cases
// because all the data members of S have a default initializer.  The front end
// now correctly inhibits the diagnostic in such situations.  This implements the
// resolution of Core issue 2366.
struct S {
  int x = 0;
  int y = 0;
};
S const s;  // Previously a spurious error.  Now okay.
