#ifndef PROGRAM_ASSEMBLY_C_API_H
#define PROGRAM_ASSEMBLY_C_API_H
struct Opaque;
typedef int (*program_callback)(int);
extern int shared_value;
extern struct Opaque *shared_pointer;
int shared_add(int value);
int opaque_read(struct Opaque *value);
int *shared_address_from_b(void);
program_callback shared_callback_from_b(void);
int unit_b_increment(void);
int unit_b_observe(int selector);
#endif
