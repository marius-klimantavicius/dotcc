typedef shared int UpcRow[2 * THREADS];
UpcRow upc_values;
UpcRow upc_outer_values[3];
UpcRow *upc_row_pointer;
int upc_thread_identity(void) { return MYTHREAD; }
int upc_threads(void) { return THREADS; }
int upc_thread_multiplier(void) { return 3 * THREADS; }
