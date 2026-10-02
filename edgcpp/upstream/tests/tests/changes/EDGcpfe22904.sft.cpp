//type:fp
//remark:[6.2] Lifetime-extended temporaries in constant expressions
// 11/4/20  [EDGcpfe/22904]
//
// Lifetime-extended temporaries in constant expressions
//
// The front end now permits the use of lifetime-extended temporaries bound to
// reference-to-const variables.
//
// This implements the resolution of the C++ standardization committee's Core
// issue 2126.
typedef const int CI[3];
constexpr CI &ci = CI{11, 22, 33};
static_assert(ci[1] == 22, "");  // Previously an error.  Now okay.
