using size_type = decltype(sizeof(0));
extern long long arena[8], other_arena[8], trace;
extern int placement_calls, allocation_calls, constructor_arguments, constructor_calls, deletion_calls, destructions, alive;
extern int tag_value, allocated_tag, deleted_tag;
extern size_type allocated_bytes;
extern void *current_pointer, *deleted_result, *deleted_argument;
extern bool return_null, allocator_failure;
void mark(int event);
void reset();
void *pointer_argument(bool fail);
int input_value(bool fail);
void *operator new(size_type bytes, void *address) noexcept;
void operator delete(void *result, void *address) noexcept;
void *operator new(size_type bytes, int tag) noexcept;
void operator delete(void *result, int tag) noexcept;
void *operator new(size_type bytes, long long tag);
void operator delete(void *result, long long tag) noexcept;

struct Value {
    Value *self;
    int value;
    Value(int input, bool fail);
    ~Value() noexcept;
    bool stable() const;
};
Value *construct_pointer(bool fail_placement, bool fail_argument, bool fail_constructor);
Value *construct_tag(bool fail_argument, bool fail_constructor);
Value *construct_throwing(bool fail_argument, bool fail_constructor);
void construct_discard(bool fail_argument, bool fail_constructor);

struct Plain { int value, other; };
extern Plain plain_source;
Plain *construct_copy(bool fail_argument);
