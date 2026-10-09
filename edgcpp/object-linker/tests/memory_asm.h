#pragma once

struct BarrierPayload { unsigned value; double real; };
extern unsigned address_calls;
extern unsigned extent_calls;
extern unsigned long extent_total;
void reset_observations();
void empty_barrier();
void input_scalar(unsigned char *data);
void output_scalar(unsigned char *data);
void inout_scalar(unsigned char *data);
void input_fixed_matrix(unsigned char *data);
void input_variable_extent(unsigned char *data, unsigned long count);
void input_variable_wide_extent(unsigned char *data, unsigned long count);
void inout_variable_extent(unsigned char *data, unsigned long count);
void multiple_operands(unsigned char *data);
void increment_address(unsigned char **data);
void record_operand(BarrierPayload *data);
