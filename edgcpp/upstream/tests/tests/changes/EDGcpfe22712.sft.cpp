//type:fp
//options_all:--gn 70400
//remark:[6.1] Incorrect disambiguation of enum-base
// 5/5/20   [EDGcpfe/22712]
//
// Incorrect disambiguation of enum-base
//
// The front end sometimes did not correctly parse enum-base specifiers because
// of a flaw in the grammar disambiguation code.
//
// This is now fixed.
int x;
enum E: __typeof(x) {}; // Previously an error.  Now okay.
