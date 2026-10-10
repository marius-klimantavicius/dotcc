namespace source_allocations {
template<class T> concept Allocated = requires(T* pointer) {
    ::new T;
    delete pointer;
    delete[] pointer;
    ::new int;
    ::new int[3];
};
template<Allocated T> struct Owner { T field = {}; };
template<class U> struct Other { U untouched = {}; };
struct Concrete { decltype(::new int) one; decltype(::new int[3]) many; };
static_assert(Allocated<int>);
int unused_runtime_declarations_do_not_execute() { return 17; }
}
int main() { source_allocations::Concrete item{}; (void)item; return source_allocations::unused_runtime_declarations_do_not_execute()==17 ? 0 : 1; }
