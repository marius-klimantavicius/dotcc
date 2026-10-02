//type: fp
//options: 
# 0 "./gnu2x-utf8str-type.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./gnu2x-utf8str-type.c"




# 1 "./c2x-utf8str-type.c" 1




_Static_assert (_Generic (u8"text", unsigned char*: 1, default: 2) == 1, "UTF-8 string literals have an unexpected type");
_Static_assert (_Generic (u8"x"[0], unsigned char: 1, default: 2) == 1, "UTF-8 string literal elements have an unexpected type");
# 6 "./gnu2x-utf8str-type.c" 2
