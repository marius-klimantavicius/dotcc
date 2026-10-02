//type:fp
//remark:[4.8] Completeness of enum types with explicit underlying types
// 7/24/13  [EDGcpfe/10301,EDGcpfe/13921,EDGcpfe/13997,EDGcpfe/14161]
//
// Completeness of enum types with explicit underlying types
//
// The front end now implements support for the resolution to the C++ standards
// committee's Core issues 803 and 977, which makes an enum type with an explicit
// underlying type complete as soon as that underlying type has been seen.
//
// In addition, for such enumeration types, the first enumerator constant was
// previously given type int until the end of the enum type's definition; it
// should have been given the specified underlying type instead.
//
// This is now fixed.
enum E: char { e = sizeof(E) };

enum F: char { f0, f1 = sizeof(f0) };
  // Previously sizeof(f0) was equivalent to sizeof(int); now it is 1.
