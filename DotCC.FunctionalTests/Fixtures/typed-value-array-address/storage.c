unsigned char key[32];
unsigned char *pointer = key;
int rows[2][3] = { { 1, 2, 3 }, { 4, 5, 6 } };
unsigned char known[32];
int untouched(void) { return key[0] == 0 && key[31] == 0; }
