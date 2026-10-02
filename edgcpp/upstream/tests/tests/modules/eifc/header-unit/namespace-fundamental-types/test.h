namespace bar {

//
// Void
//
using td_void = void;

//
// nullptr_t
//
using td_nullptr_t = decltype(nullptr);

//
// Integral Types
//

// Plain
using td_bool     = bool;
using td_char     = char;
using td_wchar_t  = wchar_t;
using td_char8_t  = char8_t;
using td_char16_t = char16_t;
using td_char32_t = char32_t;
// Signed
using td_signed_char = signed char;
using td_short       = short;
using td_int         = int;
using td_long        = long;
using td_long_long   = long long;
// Unsigned
using td_unsigned_char      = unsigned char;
using td_unsigned_short     = unsigned short;
using td_unsigned_int       = unsigned int;
using td_unsigned_long      = unsigned long;
using td_unsigned_long_long = unsigned long long;

//
// Float Types
//
using td_float       = float;
using td_double      = double;
using td_long_double = long double;

}
