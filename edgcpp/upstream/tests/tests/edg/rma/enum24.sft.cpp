//options_all:-r -x -tused
//options: --strict;cn:;cp

enum E  { zero, one, two, many = 0x7fffffff };
enum E2 { zero2 = zero, one2 = one, two2 = two };
enum E3 { zero3, one3, two3, many3 = many+1 };
enum E4 { zero4, one4 = many/many, two4 };
enum E5 { many5 = 0xffffffff };
enum E6 { many6 = many5, evenmore6, yetmore6 };
enum E7 { many7 = 0x100000000-1 };
enum E8 { x = 0x7f, y, z = y-1 };

