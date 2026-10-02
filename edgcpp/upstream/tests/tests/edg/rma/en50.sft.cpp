//options_all:-r -x -tused
//options: --strict;cn

typedef void (*fpt)(void) throw(int);
typedef void (&frt)(void) throw(int);
typedef void (ft)(void) throw(int);
typedef void (*fpt2)(void (*)(void) throw(int));
typedef void (&fpt3)(void (*)(void) throw(int));
typedef void (ft2)(void (*)(void) throw(int));

