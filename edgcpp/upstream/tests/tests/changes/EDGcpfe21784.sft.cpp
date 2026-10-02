//type:fp
//options_all:--c++17
//remark:[6.1] Class types not always correctly classified as literal/non-literal
// 2/20/20  [EDGcpfe/21784,EDGcpfe/22120]
//
// Class types not always correctly classified as literal/non-literal
//
// The front end sometimes did not correctly determine whether a class type is a
// literal type or not.
//
// That is now fixed.
struct D { ~D() {}; };  // Not a literal type (non-constexpr destructor).
struct S { union { D m; }; };  // Not a literal type (member with non-
                               // literal type).
static_assert(!__is_literal_type(S), "");
   // Previously failed.  Now okay.
