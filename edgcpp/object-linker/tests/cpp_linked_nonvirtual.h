#pragma once
struct TailBase {
    int value;
    char marker;
    TailBase(int v = 11, char m = 'b') : value(v), marker(m) {}
};
struct TailDerived : TailBase {
    char tail;
    TailDerived() : tail('d') {}
};
static_assert(__is_trivially_copyable(TailBase), "base assignment may be trivial");
static_assert(sizeof(TailBase) == 8 && sizeof(TailDerived) == 8, "LP64 tail reuse");

struct PodFirst { int first; char marker; };
struct PodSecond { int second; };
struct Multiple : PodFirst, PodSecond { int last; };

struct Empty {};
struct EmptyHolder : Empty { int value; };
struct OtherHolder : Empty { int value; };
struct RepeatedEmpty : EmptyHolder, OtherHolder { int last; };

extern int trace[16], trace_size;
void record(int value);
struct TraceFirst { TraceFirst(); ~TraceFirst(); };
struct TraceSecond { TraceSecond(); ~TraceSecond(); };
struct TraceMember { TraceMember(); ~TraceMember(); };
struct TraceDerived : TraceFirst, TraceSecond {
    TraceMember member;
    TraceDerived();
    ~TraceDerived();
};

void assign_base(TailDerived*, const TailBase&);
TailDerived roundtrip_value(TailDerived);
int read_multiple(const PodFirst*, const PodSecond*);
Multiple* from_second(PodSecond*);
const Empty* first_empty(const RepeatedEmpty*);
const Empty* second_empty(const RepeatedEmpty*);
