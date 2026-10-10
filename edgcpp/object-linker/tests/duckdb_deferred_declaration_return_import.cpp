template<class T> struct Result { T value; ~Result() {} };
struct Base { virtual Result<int> get(); virtual ~Base() {} };
struct Derived : Base {};
Base* construct(void*);
void* operator new(unsigned long, void* storage) noexcept {return storage;}
void operator delete(void*,void*) noexcept {}
Base* construct(void* storage){return new(storage) Derived;}
