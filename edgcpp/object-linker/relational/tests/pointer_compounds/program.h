const unsigned char *shift_bytes(const unsigned char *value, long offset, bool subtract);
int *shift_words(int *value, long offset, bool subtract);
int **selected_target(int **value, int *trace, int *calls);
long selected_offset(long value, int *trace, int *calls);
extern int *shared_pointer;
