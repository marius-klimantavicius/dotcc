//type:fp
//options_all:--c++11 --gn 50500
//remark:[6.2] Null pointer constants
// 11/11/20 [EDGcpfe/23533,EDGcpfe/23541]
//
// Null pointer constants
//
// In C++11 mode, the front end no longer treats as a "null pointer constant" an
// expression that names a constant-valued variable of integer type with value
// zero.
//
// This is a consequence of the resolution of Core issue 903 (treated as a defect
// report against C++11).  In permissive Microsoft modes, the prior behavior is
// retained, however.
int const zero = 0;
int f(char);
int f(const char*);
int r = f(zero);  // Previously always ambiguous because "zero" was
                  // considered a valid null pointer constant.  Now okay
                  // in C++11 (and later) modes.
