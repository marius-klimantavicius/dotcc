// Selecting a class result causes stock lowering to preserve an already-
// constructed value in execution storage while enforcing operand sequencing.
int live, constructed, destroyed;
struct StorageValue {
    int value;
    explicit StorageValue(int n):value(n) { ++live; ++constructed; }
    StorageValue(const StorageValue &other):value(other.value) { ++live; ++constructed; }
    StorageValue &operator=(const StorageValue &) = delete;
    ~StorageValue() { --live; ++destroyed; }
};
StorageValue storage_make(int n) { return StorageValue(n); }
StorageValue storage_choose(bool take_source,const StorageValue &source) {
    return take_source ? source : storage_make(17);
}
int storage_observe(bool take_source,const StorageValue &source) {
    return storage_choose(take_source,source).value;
}
struct alignas(8) TailBase {
    int value;
    explicit TailBase(int n):value(n) {}
    TailBase(const TailBase &other):value(other.value) {}
    TailBase &operator=(const TailBase &) = default;
    ~TailBase() {}
};
struct TailEnvelope : TailBase {
    int neighbor;
    TailEnvelope(int n,int sentinel):TailBase(n),neighbor(sentinel) {}
};
static_assert(sizeof(TailBase)==8 && sizeof(TailEnvelope)==8);
int main() {
    {
        StorageValue source(31);
        int before_constructed=constructed, before_destroyed=destroyed;
        if(storage_observe(true,source)!=31 || live!=1 || constructed!=before_constructed+1 || destroyed!=before_destroyed+1)return 1;
        if(storage_observe(false,source)!=17 || live!=1)return 2;
        if(source.value!=31)return 3;
    }
    if(live!=0 || constructed!=destroyed)return 4;
    {
        TailEnvelope envelope(9,0x12345678);
        TailBase replacement(27);
        static_cast<TailBase&>(envelope)=replacement;
        if(envelope.value!=27 || envelope.neighbor!=0x12345678)return 5;
    }
    return 0;
}
