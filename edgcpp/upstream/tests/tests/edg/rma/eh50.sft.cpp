//options_all:-r -x -tused
//options: --strict;cn

//test 1

typedef void (*fpt)(void) throw(int);

//test 2

typedef void (&frt)(void) throw(int);

//test 3

typedef void (ft)(void) throw(int);

//test 4

typedef void (*fpt2)(void (*)(void) throw(int));

//test 5

typedef void (&fpt3)(void (*)(void) throw(int));

//test 6

typedef void (ft2)(void (*)(void) throw(int));

