using size_type = decltype(sizeof(0));
void* operator new(size_type, void* pointer) noexcept { return pointer; }
void operator delete(void*, void*) noexcept {}

int trace;
int pointer_calls;
int deletion_calls;
size_type deleted_size;
void* deleted_pointer;

struct Member {
    int value;
    explicit Member(int input) : value(input) {}
    ~Member() noexcept { trace = trace * 10 + 2; }
};

struct Base {
    int value;
    explicit Base(int input) : value(input) {}
    virtual ~Base() noexcept { trace = trace * 10 + 3; }
    static void operator delete(void*, size_type) noexcept {}
};

struct Derived : Base {
    Member member;
    explicit Derived(int input) : Base(input), member(input) {}
    ~Derived() noexcept override { trace = trace * 10 + 1; }
    static void operator delete(void*, void*) noexcept {}
    static void operator delete(void* pointer, size_type size) noexcept {
        trace = trace * 10 + 4;
        ++deletion_calls;
        deleted_pointer = pointer;
        deleted_size = size;
    }
};

alignas(Derived) unsigned char storage[sizeof(Derived)];

Base* selected_pointer(Base* pointer) {
    ++pointer_calls;
    return pointer;
}

void destroy(Base* pointer) { delete selected_pointer(pointer); }

int main() {
    Derived* object = new (storage) Derived(17);
    Base* pointer = object;
    destroy(pointer);
    if (pointer_calls != 1 || deletion_calls != 1 || trace != 1234) return 1;
    if (deleted_pointer != object || deleted_size != sizeof(Derived)) return 2;
    destroy(nullptr);
    if (pointer_calls != 2 || deletion_calls != 1 || trace != 1234) return 3;
    return 0;
}
