/* The published dot-gen portable target is little-endian LP64/binary80.
   These constants describe the translated ABI, never compile-host OS choice. */
#ifndef EDG_PORTABLE_MATH_ABI_H
#define EDG_PORTABLE_MATH_ABI_H
#define hidden
#ifndef __LITTLE_ENDIAN
#define __LITTLE_ENDIAN 1234
#endif
#ifndef __BIG_ENDIAN
#define __BIG_ENDIAN 4321
#endif
#ifndef __BYTE_ORDER
#define __BYTE_ORDER __LITTLE_ENDIAN
#endif
#if __BYTE_ORDER != __LITTLE_ENDIAN || LDBL_MANT_DIG != 64 || LDBL_MAX_EXP != 16384
#error This selected math component requires the portable little-endian binary80 ABI
#endif
#endif
