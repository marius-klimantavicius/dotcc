//options_all:-r -x -tused
//options: --strict;cp

enum E { g, r, b };
const int i = 7;
const E ev = b;
int ai[i-ev+sizeof(int)-sizeof(char)];
double ad[ev];

