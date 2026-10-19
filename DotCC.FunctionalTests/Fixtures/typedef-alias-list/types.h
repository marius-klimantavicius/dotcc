#ifndef TYPES_H
#define TYPES_H
typedef struct Word { unsigned long low, high; } Word, Alias;
typedef union Number { unsigned long wide; unsigned char byte; } Number, NumberAlias;
typedef enum Kind { KIND=7 } Kind, KindAlias;
typedef int *Pointer, Scalar, Rows[2][3];
typedef Pointer FirstPointer, SecondPointer;
typedef int Size, Values[sizeof(Size)];
Alias transform(Word value);
#endif
