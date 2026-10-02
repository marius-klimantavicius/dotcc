//options_all:-r -x -tused
//options: --strict;cn:--diag_warning=260;cn

// ARM 7.1.1 pp. 98-99
static char* f();
char* f() { /* ... */ }

char* g();
static char* g() { /* ... */ }            // error

static int a;
int a;                                    // error

static int b;
extern int b;

int c;
static int c;                             // error

extern d;
static int d;                             // error

// and from 7.1.2, p. 105

extern void ff();
inline void ff() { /* ... */ }            // error

