extern const char digits[20];
extern int shared_values[4];
int *shared_data();
int local_arrays();
extern int zero_values[3];
int *zero_data();
void array_cleanup(int *result, bool fail);
