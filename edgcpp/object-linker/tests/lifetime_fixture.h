#ifndef LIFETIME_FIXTURE_H
#define LIFETIME_FIXTURE_H
#ifdef __cplusplus
extern "C" {
#endif
void cleanup_one(int** value);
void cleanup_two(int** value);
int cleanup_three(void* value);
void cleanup_array(int* (*value)[2]);
void cleanup_scalar(unsigned long* value);
void cleanup_write_only(int* value);
int lifetime_return(int* trace, int early);
int lifetime_failure(int* trace, int divisor, int fail_initializer);
int lifetime_divide(int divisor);
int* lifetime_pointer_initializer(int** input);
#ifdef __cplusplus
}
#endif
#endif
