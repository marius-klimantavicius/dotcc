static int allocation_calls;
static int deletion_calls;
static int address_calls;
void *operator new(unsigned long, void *address) noexcept {
    ++allocation_calls;
    return address;
}
void operator delete(void *, void *) noexcept { ++deletion_calls; }
struct Empty {};
static void *destination(void *address) {
    ++address_calls;
    return address;
}
int main() {
    Empty source;
    alignas(Empty) char storage[sizeof(Empty)];
    Empty *result = new (destination(storage)) Empty(source);
    return (void *)result != (void *)storage || allocation_calls != 1 ||
           address_calls != 1 || deletion_calls != 0;
}
