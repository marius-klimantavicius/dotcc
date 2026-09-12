/* MsQuic's -fms-extensions named-tag anonymous member. */
struct Common { int value; };
struct Container { struct Common; int other; };
int read_value(struct Container *p) { return p->value; }
