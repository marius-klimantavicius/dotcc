//type:fp
//options_all:--c23
//remark:[6.5] C23: UTF-8 character literals
// 1/16/23  [EDGcpfe/25889]
//
// C23: UTF-8 character literals
//
// As described in WG14 paper N2418, the front end by default now accepts
// UTF-8 character literals in C23 mode.  These literals have type unsigned
// char, in contrast to char in C++17 and char8_t in C++20.  This support can
// be explicitly controlled using the --[no_]utf8_char_literals command-line
// option.
unsigned char c = u8'x';   // Now accepted by default in C23 mode
