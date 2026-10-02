//type:fp
//options_all:--c++17
//remark:[4.11] C++17 compatibility: Add standard attributes to namespaces and enumerators
// 6/26/15  [EDGcpfe/16270]
//
// C++17 compatibility: Add standard attributes to namespaces and enumerators
//
// As outlined in N4266, standard attributes are now permitted on namespaces
// and enumerators.  Only the "deprecated" attribute is currently supported in
// these locations.  This feature is also enabled when microsoft_version >= 1900.
namespace [[deprecated]] N {
  int x = 0;
}
enum E { y [[deprecated]] };
int z = N::x + y;     // Generates warnings for uses of N and y.
