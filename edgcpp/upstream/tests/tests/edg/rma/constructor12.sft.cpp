//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cn

struct A { inline inline A(); };
struct B { __declspec(xxx) B(); };

