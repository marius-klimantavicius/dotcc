#if u8'a' > -1
byte_positive
#else
byte_unsigned
#endif
#if u'a' > -1
utf16_positive
#else
utf16_unsigned
#endif
#if U'a' > -1
utf32_positive
#else
utf32_unsigned
#endif
#if U'\xffffffff' + U'\1' == 0
utf32_wrap32
#else
utf32_no_wrap32
#endif
#if +U'a' == 97
unary_value
#endif
