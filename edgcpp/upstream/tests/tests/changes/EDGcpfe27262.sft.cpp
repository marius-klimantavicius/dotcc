//type:fp
//options_all:--gn 999999
//remark:[6.7] GNU C++ compatibility: Trailing return types
// 8/26/24  [EDGcpfe/27262,EDGcpfe/27540]
//
// GNU C++ compatibility: Trailing return types
//
// When declaring a function with a trailing return type, the type preceding the
// declaration must be just "auto".  However, GCC also accepts "const" and
// "volatile" qualifiers on that "auto", and the front end now emulates that
// behavior with a warning.
auto const g()->int;  // Ordinarily an error, but now accepted in GNU C++
                      // mode with a warning.
