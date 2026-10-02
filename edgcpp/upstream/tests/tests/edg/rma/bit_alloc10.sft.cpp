//options_all:-r -x -tused
//options: --strict;cn

// Microsoft allocation mode
struct A { char x,y; long :0; }; 
    // only 2 bytes. Alignment is 1. :0 is ignored
struct B { char a,b,c, d:8; long :0; } 
    // 4 bytes, but overall alignment is 4. :0 is effective.
struct C { char a:3; short :0; long :0; }
    // 2 bytes, alignment==2. The first :0 is effective, but 
    // not the second one.


