// A user-declared trivial default constructor creates original rollback metadata,
// but stock dynamic-array value initialization only zeroes the allocated bytes.
using Size=decltype(sizeof(0));
alignas(16) unsigned char arena[256];
int allocations,deallocations,fail_allocation,count_evaluations;
Size count_once(){++count_evaluations;return 3;}
void *operator new[](Size bytes){++allocations;if(fail_allocation)throw 41;if(bytes>sizeof(arena))throw 42;for(Size i=0;i<bytes;++i)arena[i]=0xa5;return arena;}
void operator delete[](void *p) noexcept {if(p==arena)++deallocations;}
struct Value {int value; Value()=default;};
Value *make_values(Size count){return new Value[count]();}
int main(){Value *p=make_values(count_once());for(int i=0;i<3;++i)if(p[i].value)return 1;delete[]p;if(allocations!=1||deallocations!=1||count_evaluations!=1)return 2;fail_allocation=1;try{make_values(2);return 3;}catch(int n){if(n!=41)return 4;}return allocations!=2||deallocations!=1;}
