//type:fp
//options:--c++17:--ms_c++17:--c++20 -DHAS_CHAR8_T:--c++20 --no_char8_t:--ms_c++20 -DHAS_CHAR8_T:--ms_c++20 --no_char8_t

#ifdef HAS_CHAR8_T
char8_t c = u8' ';
const char8_t *s = u8"";
const decltype(u8' ') *s2 = u8"";

#ifndef __cpp_char8_t
#error Unexpected
#endif
#else
char c = u8' ';
const char *s = u8"";
const decltype(u8' ') *s2 = "";

int char8_t;

#ifdef __cpp_char8_t
#error Unexpected
#endif
#endif
