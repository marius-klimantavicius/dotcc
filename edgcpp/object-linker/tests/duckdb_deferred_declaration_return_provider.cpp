template<class T> struct Result { T value; ~Result() {} };
struct Base { virtual Result<int> get(); virtual ~Base() {} };
struct Derived : Base {};
Base* construct(void*);
Result<int> Base::get(){return {41};}
int main(){alignas(Derived) unsigned char storage[sizeof(Derived)]; auto value=construct(storage);int result=value->get().value;value->~Base();return result==41?0:1;}

extern "C" void free(void*);
void operator delete(void* memory) noexcept { free(memory); }
void operator delete(void* memory, unsigned long) noexcept { free(memory); }
