#ifndef PROGRAM_ASSEMBLY_CPP_API_H
#define PROGRAM_ASSEMBLY_CPP_API_H
namespace captured_program {
extern int shared_value;
int choose(int value);
unsigned choose(unsigned value);
}
extern "C" {
int cpp_unit_a(int value);
int cpp_unit_a_observe(int selector);
int *cpp_shared_address_from_a(void);
int (*cpp_shared_callback_from_a(void))(int);
}
#endif
