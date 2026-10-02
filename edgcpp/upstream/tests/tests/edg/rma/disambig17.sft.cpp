//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cn

class t1 {};
class t2 {};
// Declare t1 as a function taking and int and returning int.
t1(int x) {}
// Declare foo as a function with an unnamed parameter of
// type function taking int and returning t2. (Semantics converts the
// parameter to be a pointer to function.)
int foo(t2(int x));
bar () {
    // (Incorrectly) allocate a function taking int and
    // returning t2.
    new (t2(int x));
}

