#ifndef POINTER_FIXTURE_H
#define POINTER_FIXTURE_H
typedef int* IntPointer;
typedef const int* ConstIntPointer;
#ifdef __cplusplus
extern "C" {
#endif
int pointer_read(ConstIntPointer value);
void pointer_write(IntPointer value, int next);
int* pointer_choose(int* first, int* second, int choice);
void pointer_replace(int** slot, int* next);
unsigned long long* pointer_wide(unsigned long long* value);
int pointer_post_increment(int* value);
#ifdef __cplusplus
}
#endif
#endif
