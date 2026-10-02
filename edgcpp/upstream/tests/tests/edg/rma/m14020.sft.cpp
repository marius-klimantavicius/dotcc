//options_all:-r -x -tused
//options: --strict;cn:;cn

// #020 _142p61c template name unique, can't refer to other function

template <class T> class x { T i; };
int x() { return 0; }; // error - x not unique


