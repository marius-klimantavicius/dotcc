//type:fp
//options_all:--microsoft_v 1921 --ms_c++17
//remark:[6.5] Nonstandard implicit conversion from scoped enum type
// 4/28/23  [EDGcpfe/24594,EDGcpfe/26275]
//
// Nonstandard implicit conversion from scoped enum type
//
// In Microsoft C++ mode and in the GNU C++ mode with gnu_version between 50000
// and 59999, the front end now accepts (with a warning) scoped enum type values
// as initializers for unrelated enumerator constants.
enum class X { x };
enum class Y { y = X::x };  // Now accepted with a warning in some modes.
