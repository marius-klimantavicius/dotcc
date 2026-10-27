#ifndef AGGREGATE_BYTE_DATA_STORAGE_PROBE_H
#define AGGREGATE_BYTE_DATA_STORAGE_PROBE_H

int storage_size(int table);
unsigned char storage_byte(int table, int index);
unsigned long storage_static_address(void);
unsigned long storage_tls_address(void);
int storage_static_alignment(void);
int storage_tls_alignment(void);
int storage_static_check(int first);
void storage_static_set(int first);
int storage_tls_check(unsigned first);
unsigned storage_tls_step(void);
int storage_loop(int rounds, int depth);

#endif
