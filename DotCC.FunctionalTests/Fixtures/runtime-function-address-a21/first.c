#include "contract.h"
static Lookup saved_lookup = find_record;
static int evaluations;
static const PublicRecord *next_record(const PublicRecord *p) { ++evaluations; return p; }
Lookup first_address(void) { return &find_record; }
int verify(void) {
    evaluations=0;
    PublicRecord records[2]={{17},{29}};
    Lookup lookup=find_record;
    Lookup (*factory)(Lookup)=callback_result;
    unsigned short (*narrow)(unsigned short)=narrow_result;
    _Bool (*truth)(int)=truth_result;
    intptr_t (*native)(intptr_t)=native_result;
    void (*touch)(PublicRecord *)=touch_record;
    if (!lookup || lookup!=&find_record || lookup!=saved_lookup || lookup!=first_address() || lookup!=second_address()) return 1;
    if ((Lookup)(void*)lookup!=lookup || factory(lookup)!=lookup || factory(0)!=0) return 2;
    const PublicRecord *p=lookup(next_record(records),1,-7);
    if (p!=records+1 || p->value!=29 || evaluations!=1) return 3;
    if (lookup(0,0,0)!=0 || lookup(records,65535,-128)!=0) return 4;
    if (0) lookup(next_record(records),0,0);
    if (evaluations!=1) return 5;
    if (narrow(65535)!=65535 || !truth(-3) || truth(0) || native((intptr_t)-1234567)!=(intptr_t)-1234567) return 6;
    touch(records);
    if (records[0].value!=18) return 7;
    return 0;
}
