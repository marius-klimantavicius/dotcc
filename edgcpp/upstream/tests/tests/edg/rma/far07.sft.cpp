//options_all:-r -x -tused
//options: --strict;cn

typedef int I;
typedef int near I; // error
typedef int J;
typedef int far J;  // error
typedef int near K;
typedef int near K;  // okay
typedef int far L;
typedef int far L;  // okay


