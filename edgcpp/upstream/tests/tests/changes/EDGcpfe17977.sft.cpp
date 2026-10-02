//type:fp
//options_all:--gnu_version 60200 --c++14
//remark:[6.3] GNU C++ compatibility: Abbreviated function templates in pre-C++20 modes
// 8/9/21   [EDGcpfe/17977,EDGcpfe/18960,EDGcpfe/19153,EDGcpfe/20794,
//           EDGcpfe/22026]
//
// GNU C++ compatibility: Abbreviated function templates in pre-C++20 modes
//
// The front end now accepts abbreviated function template syntax (a C++20
// feature that was added to the language along with constrained templates and
// concepts) in GNU C++14 (and later) modes with gnu_version >= 40900.
auto next(auto p) { return ++p; }  // Now accepted in more GNU modes.
