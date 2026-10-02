//type:fp
//options_all:--clang --c++17
//remark:[6.1] ext_vector_type attribute and typedefs
// 7/14/20  [EDGcpfe/22022]
//
// ext_vector_type attribute and typedefs
//
// In Clang mode, the ext_vector_type attribute did not produce a correct size
// for the resulting type if the underlying type is a typedef.
//
// That is now fixed.
typedef unsigned char uchar;
typedef uchar uchar2 __attribute__((ext_vector_type(2)));
typedef unsigned char ucharV2 __attribute__((ext_vector_type(2)));
static_assert(sizeof(uchar2) == sizeof(ucharV2), "Unexpected");
  // Previously failed.  Now okay.
