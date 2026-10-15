struct Opaque { int value; };
struct Opaque opaque_storage = { 9 };
struct Opaque *shared_pointer = &opaque_storage;
int shared_value = 11;
int shared_add(const int value) { return value + 2; }
int differently_named(int value) __asm__("assembly_recorded_name");
int differently_named(int value) { return value; }
static int local_value = 7;
static int local_helper(int value) { return value + local_value; }
int read_second(void) { return local_helper(shared_pointer->value); }
