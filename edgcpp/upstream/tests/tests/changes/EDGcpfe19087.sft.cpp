//type:fp
//options_all:--auto_storage --auto_storage --c++14
//remark:[5.0] Spurious error on decltype(auto)  with --auto_storage
// 4/11/18  [EDGcpfe/19087]
//
// Spurious error on decltype(auto)  with --auto_storage
//
// Use of decltype(auto) would result in a compilation error when --auto_storage
// was in effect. That has now been fixed.
// --auto_storage:
decltype(auto) x = 2; // Previously resulted in invalid combination of type
                      // specifiers
