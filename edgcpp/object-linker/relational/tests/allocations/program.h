using size_type = decltype(sizeof(0));
extern size_type allocation_bytes;
extern int allocation_calls;
extern int count_calls;
extern int failure_calls;
int next_count(int value);
char *allocate_bytes(size_type count);
