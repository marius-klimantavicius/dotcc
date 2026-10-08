#include "cpp_linked_nonvirtual.h"

int trace[16], trace_size;
void record(int value) { trace[trace_size++] = value; }
TraceFirst::TraceFirst() { record(1); }
TraceFirst::~TraceFirst() { record(-1); }
TraceSecond::TraceSecond() { record(2); }
TraceSecond::~TraceSecond() { record(-2); }
TraceMember::TraceMember() { record(3); }
TraceMember::~TraceMember() { record(-3); }
TraceDerived::TraceDerived() { record(4); }
TraceDerived::~TraceDerived() { record(-4); }

void assign_base(TailDerived* target, const TailBase& source) {
    static_cast<TailBase&>(*target) = source;
}
TailDerived roundtrip_value(TailDerived value) {
    value.value += 7;
    value.tail = 'z';
    return value;
}
int read_multiple(const PodFirst* first, const PodSecond* second) {
    return first->first + first->marker + second->second;
}
Multiple* from_second(PodSecond* value) { return static_cast<Multiple*>(value); }
const Empty* first_empty(const RepeatedEmpty* value) {
    return static_cast<const EmptyHolder*>(value);
}
const Empty* second_empty(const RepeatedEmpty* value) {
    return static_cast<const OtherHolder*>(value);
}
