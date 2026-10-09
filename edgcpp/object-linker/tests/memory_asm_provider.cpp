#include "memory_asm.h"

unsigned address_calls;
unsigned extent_calls;
unsigned long extent_total;

void reset_observations() {
  address_calls = 0;
  extent_calls = 0;
  extent_total = 0;
}

static unsigned char *operand_address(unsigned char *data) {
  ++address_calls;
  return data;
}

static unsigned long operand_extent(unsigned long count) {
  ++extent_calls;
  extent_total += count;
  return count;
}

void empty_barrier() {
  __asm__ __volatile__("" ::: "memory");
  __asm__("" :);
}

void input_scalar(unsigned char *data) {
  __asm__ __volatile__("" : : "m"(*operand_address(data)));
}

void output_scalar(unsigned char *data) {
  __asm__ __volatile__("" : "=m"(*operand_address(data)));
}

void inout_scalar(unsigned char *data) {
  __asm__ __volatile__("" : "+m"(*operand_address(data)) : : "memory");
}

void input_fixed_matrix(unsigned char *data) {
  typedef unsigned short Matrix[2][3];
  __asm__ __volatile__("" : : "m"(*(Matrix *)operand_address(data)));
}

void input_variable_extent(unsigned char *data, unsigned long count) {
  __asm__ __volatile__("" : : "m"(*(unsigned char (*)[operand_extent(count)])operand_address(data)) : "memory");
}

void input_variable_wide_extent(unsigned char *data, unsigned long count) {
  __asm__ __volatile__("" : : "m"(*(unsigned short (*)[operand_extent(count)])operand_address(data)));
}

void inout_variable_extent(unsigned char *data, unsigned long count) {
  __asm__ __volatile__("" : "+m"(*(unsigned char (*)[operand_extent(count)])operand_address(data)));
}

void multiple_operands(unsigned char *data) {
  __asm__ __volatile__("" : [destination] "=m"(*operand_address(data)),
                           [update] "+m"(*operand_address(data + 1))
                         : [source] "m"(*operand_address(data + 2)) : "memory");
}

void increment_address(unsigned char **data) {
  __asm__ __volatile__("" : "+m"(*(*data)++));
}

void record_operand(BarrierPayload *data) {
  __asm__ __volatile__("" : "+m"(*data));
}
