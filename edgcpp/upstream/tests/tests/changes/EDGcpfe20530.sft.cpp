//type:fp
//options_all:--c++20 --c++20 --c++20
//remark:[5.1] C++20: char8_t type and keyword
// 3/14/19  [EDGcpfe/20530,EDGcpfe/20644]
//
// C++20: char8_t type and keyword
//
// The front end has been enhanced to support the char8_t type and keyword as
// described in Committee document P0482R6.  The char8_t keyword and the new
// type for character and string literals with a u8 prefix are enabled by
// the --c++20 command line option and controlled by the new --[no_]char8_t
// command-line option.
//
// Note that the IA-64 mangling encoding for the char8_t type is "Du" which had
// previously been used (as an EDG extension) to mangle __underlying_type.  As
// a result of these changes, when ABI_COMPATIBILITY_VERSION >= 510,
// __underlying_type will be mangled as "U3eut" (see EDGcpfe/20541) in all
// (IA-64 ABI) cases.
const char8_t str[] = u8"abc";  // Accepted with --c++20
const char s2[] = u8"xyz";      // Error when the 5.1 change was made;
                                // accepted again in later versions.
