using size_t = decltype(sizeof(0));
static int allocations, releases, fail_allocation;
void* operator new[](size_t, void* storage) { ++allocations; if(fail_allocation) throw 1; return storage; }
void operator delete[](void*,void*) noexcept { ++releases; }
struct Trivial { Trivial() = default; int value; };
// The dynamic-bound variant additionally emits the standard bad-array-length
// provider. Its generation evidence is retained separately until runtime closure.
Trivial* create(void* storage, size_t count) {
#ifdef DOTOBJ_DYNAMIC_ARRAY_BOUND
 return new(storage) Trivial[count];
#else
 return new(storage) Trivial[3];
#endif
}
int consume(Trivial* pointer) { pointer[0].value=7; throw 2; }
int main(){
 alignas(Trivial) unsigned char buffer[4*sizeof(Trivial)];
 auto pointer=create(buffer,3);for(int i=0;i<3;++i)pointer[i].value=i+1;
 if(pointer[2].value!=3 || allocations!=1 || releases) return 1;
 fail_allocation=1;try{create(buffer,3);return 2;}catch(int v){if(v!=1 || releases)return 3;}
 fail_allocation=0;try{consume(create(buffer,3));return 4;}catch(int v){if(v!=2 || releases)return 5;}
 return allocations==3?0:6;
}
