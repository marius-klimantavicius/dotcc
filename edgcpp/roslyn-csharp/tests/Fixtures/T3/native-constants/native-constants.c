/* Genuine application: retained native-width constants and address destinations. */
typedef void (*Destructor)(void *);

static void increment(void *cookie) {
    *(int *)cookie += 1;
}

/* The destination is deliberately a data pointer, not the routine-pointer view. */
void *stored_function_address = (void *)increment;
Destructor stored_routine_address = increment;
int observations[5];

int fixture(void) {
    Destructor sentinel = (Destructor)-1;
    int value = 40;
    ((Destructor)stored_function_address)(&value);
    stored_routine_address(&value);
    observations[0] = (unsigned long)sentinel == 18446744073709551615UL;
    observations[1] = sentinel == (Destructor)-1;
    observations[2] = stored_function_address == (void *)stored_routine_address;
    observations[3] = stored_routine_address == increment;
    observations[4] = value;
    return value + observations[0] + 2 * observations[1]
        + 4 * observations[2] + 8 * observations[3];
}
