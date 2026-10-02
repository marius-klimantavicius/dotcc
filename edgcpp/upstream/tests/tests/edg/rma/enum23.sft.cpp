//options_all:-r -x -tused
//options: --strict;cn:;cp

enum A { xa = (char)0x7f,
         ya,
         za = (int)(ya > -1) };
enum B { xb = (unsigned char)0xff,
         yb,
         zb = (int)(yb > -1) };
enum C { xc = (short)0x7fff,
         yc,
         zc = (int)(yc > -1) };
enum D { xd = (unsigned short)0xffff,
         yd,
         zd = (int)(yd > -1) };
enum E { xe = (int)0x7fffffff,
         ye,
         ze = (int)(ye > -1) };
enum F { xf = (unsigned int)0xffffffff,
         yf,
         zf = (int)(yf > -1) };
enum G { xg = (long long)0x7fffffffffffffff,
         yg,
         zg = (int)(yg > -1) };

