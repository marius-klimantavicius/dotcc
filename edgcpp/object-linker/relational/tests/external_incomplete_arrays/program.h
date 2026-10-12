using word = unsigned long long;
extern const word values[];
extern const unsigned char bytes[];
extern const int matrix[][2];
const word* values_address();
const unsigned char* bytes_address();
word read_value(int index);
int run_arrays();
