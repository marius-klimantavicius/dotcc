#include <stdint.h>
uint16_t read_native16(const void*);
uint32_t read_be32(const void*);
uint64_t read_le64(const void*);
void write_le16(void*,uint16_t);
void write_be32(void*,uint32_t);
void write_native64(void*,uint64_t);
uint32_t swap32(uint32_t);
uint64_t swap64(uint64_t);
unsigned clz32(uint32_t);
unsigned ctz32(uint32_t);
unsigned clz64(uint64_t);
unsigned ctz64(uint64_t);
unsigned highbit(uint32_t);
unsigned commonbytes(uint64_t);
unsigned little_unsigned(void);
int little_signed(void);
