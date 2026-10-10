#include "cpp_production_cross.h"
extern "C" int production_cross_catch(int value) {
    try {
        try { production_cross_throw(value); }
        catch(ProductionShared &object) { record_event(20+object.value); ++object.value; throw; }
    } catch(const ProductionShared &object) { record_event(30+object.value); return object.value; }
    return -1;
}
