//type:fp
//options_all:--c++17
//remark:[4.14] C++17 compatibility: Direct-list-initialization of enums from numeric values
// 4/4/17   [EDGcpfe/17693,EDGcpfe/17993]
//
// C++17 compatibility: Direct-list-initialization of enums from numeric values
//
// The front end now supports the C++17 feature described in Committee
// document P0138R2.  This capability allows conversion of a numeric value to
// an enumeration type if the enumeration has a fixed underlying type, the
// initialization is direct-list-initialization, and the conversion does not
// involve narrowing.
//
// This feature is also enabled in Microsoft mode when microsoft_version is
// at least 1911.
enum byte : unsigned char { };
byte b{ 42 };   // Permitted by C++17
