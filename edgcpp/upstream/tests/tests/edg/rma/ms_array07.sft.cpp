//options_all:-r -x -tused
//options: --microsoft -n;cp

int a[];
int a[3];

extern int b[];
static int b[3];

extern int c[];
extern int c[3];

int d[];
static int d[3];

int e[3];
int e[];

extern int f[3];
extern int f[];

static int g[3];
int g[];

struct S;
struct S s[];
struct S { int i; };
struct S s[3];

