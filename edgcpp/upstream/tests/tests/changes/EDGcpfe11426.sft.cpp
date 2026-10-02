//type:fn
//options_all:--c++11
//remark:[4.4] Unnamed scoped enumerations
// 5/6/11   [EDGcpfe/11426]
//
// Unnamed scoped enumerations
//
// The front end now reports an error for attempts to declare an unnamed scoped
// enumeration type (in C++0x mode and certain Microsoft modes).
enum class { e };  // Now triggers an error in C++0x mode.
