//options_all:-r -x -tused
//options: --strict;cn

struct OUTER
{
    friend struct INNER { int member; };	/* ERROR */
};

int main () { return 1; }



