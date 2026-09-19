struct entry { const char *name; unsigned mask; unsigned value; };
struct table { const char *format; struct entry entries[]; };
extern struct table exported;
int read_static(void);
