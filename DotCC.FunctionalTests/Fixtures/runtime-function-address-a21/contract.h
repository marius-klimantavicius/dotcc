#ifndef A21_CONTRACT_H
#define A21_CONTRACT_H
#include <stdint.h>
typedef struct { int value; } PublicRecord;
typedef const PublicRecord *(*Lookup)(const PublicRecord *, unsigned short, signed char);
const PublicRecord *find_record(const PublicRecord *, unsigned short, signed char);
unsigned short narrow_result(unsigned short);
_Bool truth_result(int);
intptr_t native_result(intptr_t);
void touch_record(PublicRecord *);
Lookup callback_result(Lookup);
Lookup first_address(void);
Lookup second_address(void);
int verify(void);
#endif
