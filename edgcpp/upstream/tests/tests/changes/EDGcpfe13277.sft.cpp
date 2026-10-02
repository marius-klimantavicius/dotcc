//type:fn
//remark:[5.0] Explicit default constructors
// 6/15/18  [EDGcpfe/13277,EDGcpfe/19047,EDGcpfe/19572,EDGcpfe/19776]
//
// Explicit default constructors
//
// The front end now ignores explicit default constructors when considering
// default initialization in copy-initialization contexts.
//
// This implements the resolution to core issue 1518.
struct S { explicit S() {} };
S x[3] = {};  // Previously accepted.  Now an error.
S y({});  // Previously accepted.  Now an error.
