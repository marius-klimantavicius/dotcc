//options_all:-r -x -tused
//options: --strict;cn:--anachronisms;cp



// EDGqa01258
// cv-qualifier on old-style param list when qualifiers removed
void addscalar(const int n, int m) { }
void addscalar2(n,m) const int n, m; { }


