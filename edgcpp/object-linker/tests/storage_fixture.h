#ifndef DOTOBJ_STORAGE_FIXTURE_H
#define DOTOBJ_STORAGE_FIXTURE_H

typedef int (*operation)(int);
struct Entry { const char *name; operation apply; int bias; };
union Word { unsigned int value; unsigned char bytes[4]; };
struct Flags { unsigned int count:5; signed int delta:6; unsigned int ready:1; };
struct Opaque;
extern int counter;
extern int *counter_pointer;
extern struct Entry operations[2];
extern union Word word;
extern struct Flags flags;
extern __thread int thread_counter;
int increment(int);
int run_operation(int, int);
int static_counter(void);
struct Opaque *opaque_instance(void);
int opaque_read(struct Opaque *);

#endif
