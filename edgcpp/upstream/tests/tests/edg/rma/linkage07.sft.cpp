//options_all:-r -x -tused
//options: --strict;cp

int f (void);

extern "C++" int f (void);

int f (void) { return 1; }


