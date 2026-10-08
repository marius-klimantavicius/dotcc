// Deliberately discover different pointer types before the shared header.
void** pointer_opaque_slot(void** value) { return value; }
#include "pointer_fixture.h"

int pointer_read(ConstIntPointer value) { return *value; }
void pointer_write(IntPointer value, int next) { *value = next; }
int* pointer_choose(int* first, int* second, int choice)
{
    return choice ? first : second;
}
void pointer_replace(int** slot, int* next) { *slot = next; }
unsigned long long* pointer_wide(unsigned long long* value) { return value; }
int pointer_post_increment(int* value) { return (*value)++; }
