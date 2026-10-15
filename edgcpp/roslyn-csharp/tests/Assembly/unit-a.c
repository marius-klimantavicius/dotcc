struct Opaque;
extern struct Opaque *shared_pointer;
extern int shared_value;
extern int shared_add(int value);
extern int read_second(void);
extern int source_renamed(int value) __asm__("assembly_recorded_name");
static int local_value = 3;
static int local_helper(int value) { return value + local_value; }
int assembled_entry(void)
{
    return local_helper(shared_add(shared_value)) + read_second() + (shared_pointer != 0 ? 0 : 100) + source_renamed(0);
}
