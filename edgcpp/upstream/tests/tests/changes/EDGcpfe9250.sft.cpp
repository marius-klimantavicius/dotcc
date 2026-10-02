//type:fn
//options_all:--c++11
//remark:[4.5] C++11 and Microsoft compatibility: Opaque enum declarations
// 6/20/12  [EDGcpfe/9250,EDGcpfe/11871,EDGcpfe/12413]
//
// C++11 and Microsoft compatibility: Opaque enum declarations
//
// In C++11 mode and in Microsoft mode with microsoft_version >= 1700, the front
// end now accepts "opaque enumeration declarations"; i.e., declarations of
// enumeration types with a known underlying type (hence, complete), but
// without a definition.
enum E1: char;  // Now okay in some modes, including strict C++11 mode.
enum class E2;  // Ditto (underlying type is int).
enum E3;        // Accepted when the 4.5 change was made (except in strict
                // modes).  Later versions diagnose this as a nonstandard
                // forward declaration of an enum type.
