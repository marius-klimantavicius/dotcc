//type:fp
//remark:[4.4] Abort on qualified class declaration with attributes
// 4/27/11  [EDGcpfe/11658]
//
// Abort on qualified class declaration with attributes
//
// When GENERATE_SOURCE_SEQUENCE_LISTS is TRUE, the front end could previously
// abort with an internal error in attach_tag_attributes when parsing a class
// declaration (but not a definition) using a qualified name and with explicit
// attributes.
//
// This is now fixed.
namespace N { struct S {}; }
struct __declspec(dllimport) N::S;
   // Previously triggered an internal error in Microsoft C++ mode.
