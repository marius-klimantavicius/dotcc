//options_all:-r -x -tused
//options: --strict;cp

struct ios { int a; };
struct istream : virtual ios { int b; };
struct ostream : virtual ios { int c; };
struct iostream: istream, ostream { int d; };
struct fstreambase : virtual ios { int e; };
struct fstream : fstreambase, iostream { int f; };

