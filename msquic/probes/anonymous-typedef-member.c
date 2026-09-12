/* MsQuic builds with -fms-extensions on Linux. */
typedef struct Common { int value; } Common;
typedef struct Container { Common; int other; } Container;
int read_value(Container *p) { return p->value; }
