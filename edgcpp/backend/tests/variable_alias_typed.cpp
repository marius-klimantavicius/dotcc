typedef unsigned long size_t;
extern "C" int puts(const char*);
extern "C" void abort(void);
void* operator new(size_t, void* p) noexcept { return p; }
void operator delete(void*, void*) noexcept {}
static int constructions;
static int destructions;
struct FinalCheck { ~FinalCheck() { if (constructions != 1 || destructions != 1) abort(); } };
static FinalCheck final_check;
namespace demo {
struct alignas(32) Object {
 int value;
 Object(int input): value(input) { ++constructions; }
 ~Object() { ++destructions; }
 int read() const { return value; }
};
}
extern "C" { alignas(demo::Object) unsigned char object_storage[sizeof(demo::Object)] = {}; }
namespace demo {
extern Object object __attribute__((alias("object_storage")));
extern Object view __attribute__((alias("object_storage")));
}
int main() {
 if (constructions != 0 || destructions != 0) return 1;
 void* volatile storage_address = object_storage;
 void* volatile object_address = &demo::object;
 void* volatile view_address = &demo::view;
 if (object_address != storage_address || view_address != storage_address) return 2;
 if (((size_t)&demo::object & 31) != 0) return 3;
 new(object_storage) demo::Object(57);
 if (constructions != 1 || destructions != 0 || demo::object.read() != 57 || demo::view.read() != 57) return 4;
 demo::object.value=73;
 if (((demo::Object*)object_storage)->value != 73 || demo::view.value != 73) return 5;
 demo::object.~Object();
 if (constructions != 1 || destructions != 1) return 6;
 puts("typed variable aliases: passed");
 return 0;
}
