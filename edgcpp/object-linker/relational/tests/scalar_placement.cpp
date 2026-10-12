using size_type = decltype(sizeof(0));
int allocation_calls;
int placement_calls;
int cleanup_calls;
void* operator new(size_type, void* pointer) noexcept {
    ++allocation_calls;
    return pointer;
}
void operator delete(void*, void*) noexcept { ++cleanup_calls; }
alignas(int) unsigned char storage[3 * sizeof(int)];
void* slot(size_type offset) {
    ++placement_calls;
    return storage + offset;
}
template<class T, class... Args>
T* construct(T* pointer, Args&&... args) {
    return new (static_cast<void*>(pointer)) T(static_cast<Args&&>(args)...);
}
template<class T>
T* construct_constant(T* pointer) {
    return new (static_cast<void*>(pointer)) T(17);
}
int main() {
    int* zero = construct(static_cast<int*>(slot(0)));
    int* constant = construct_constant(static_cast<int*>(slot(sizeof(int))));
    int input = 29;
    int* expression = construct(static_cast<int*>(slot(2 * sizeof(int))), input);
    if (*zero != 0 || *constant != 17 || *expression != 29) return 1;
    if (placement_calls != 3 || allocation_calls != 3 || cleanup_calls != 0) return 2;
    return 0;
}
