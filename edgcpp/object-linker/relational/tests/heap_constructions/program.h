using size_type = decltype(sizeof(0));

extern long long arena[16], other_arena[16], trace;
extern int allocation_calls, argument_calls, construction_calls, deletion_calls, destruction_calls;
extern int allocation_mode;
extern size_type allocated_bytes, deleted_bytes;
extern void *current_storage, *deleted_storage;
void reset();
int input_value(bool fail);

struct Value {
    Value *self;
    int value;
    Value(int input, bool fail);
    ~Value() noexcept;
    static void *operator new(size_type bytes);
    static void operator delete(void *storage, size_type bytes) noexcept;
};
struct Nullable {
    Nullable *self;
    int value;
    Nullable(int input);
    static void *operator new(size_type bytes) noexcept;
    static void operator delete(void *storage) noexcept;
};
Value *construct_value(bool fail_argument, bool fail_constructor);
Nullable *construct_nullable(bool fail_argument);
