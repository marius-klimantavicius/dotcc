extern int process_value;
int peer_value = 53;
int *peer_reference = &process_value;
static int local_value = 41;
int *second_local_address(void) { return &local_value; }
