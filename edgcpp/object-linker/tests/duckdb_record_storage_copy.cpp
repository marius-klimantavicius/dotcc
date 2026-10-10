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
// Lambda captures and implicit member copies use stock initialization factories.
// Count the actual source operations so execution-storage copies cannot invent
// extra constructors or duplicate destruction of the captured owner.
int owner_live, owner_constructed, owner_destroyed, owner_moves, make_calls;
struct MoveStorage {
    int value;
    explicit MoveStorage(int n):value(n) { ++owner_live; ++owner_constructed; }
    MoveStorage(const MoveStorage &) = delete;
    MoveStorage &operator=(const MoveStorage &) = delete;
    MoveStorage(MoveStorage &&other):value(other.value) {
        other.value=-1; ++owner_live; ++owner_constructed; ++owner_moves;
    }
    ~MoveStorage() { --owner_live; ++owner_destroyed; }
};
MoveStorage make_owner(int n) { ++make_calls; return MoveStorage(n); }
template<class F> struct CaptureStorage {
    F callback;
    explicit CaptureStorage(F &&source):callback(static_cast<F&&>(source)) {}
    int read() { return callback(); }
};
template<class F> CaptureStorage<F> capture_storage(F &&source) {
    return CaptureStorage<F>(static_cast<F&&>(source));
}
int nullable_assignments;
template<class T> struct NullableStorage {
    bool empty;
    T value;
    explicit NullableStorage(T n):empty(false),value(n) {}
    NullableStorage(const NullableStorage &) = default;
    NullableStorage &operator=(const NullableStorage &other) {
        ++nullable_assignments;
        empty=other.empty;
        if(!empty)value=other.value;
        return *this;
    }
};
struct NullableEnvelope {
    NullableStorage<int> value;
    StorageValue owner;
    explicit NullableEnvelope(int n):value(n),owner(n+1) {}
};
// A condition declaration converting a derived move-only owner through an
// inherited bool operator produces an initialization-storage temporary.
int pointer_destructions;
template<class T> struct StoragePointerBase {
    T *pointer;
    explicit StoragePointerBase(T *p):pointer(p) {}
    StoragePointerBase(const StoragePointerBase &) = delete;
    StoragePointerBase &operator=(const StoragePointerBase &) = delete;
    StoragePointerBase(StoragePointerBase &&other):pointer(other.pointer) { other.pointer=nullptr; }
    ~StoragePointerBase() { ++pointer_destructions; }
    explicit operator bool() const { return pointer!=nullptr; }
};
template<class T> struct StoragePointer : StoragePointerBase<T> {
    using StoragePointerBase<T>::StoragePointerBase;
};
struct StoragePointerOwner {
    StoragePointer<int> value;
    explicit StoragePointerOwner(int *p):value(p) {}
};
int read_storage_pointer(StoragePointerOwner &owner) {
    if(auto &pointer=owner.value)return *pointer.pointer;
    return -1;
}
int check_initialization_storage() {
    {
        auto callback=[owner=make_owner(47)] { return owner.value; };
        if(make_calls!=1 || owner_live!=1 || callback()!=47)return 6;
        auto saved=capture_storage(static_cast<decltype(callback)&&>(callback));
        if(saved.read()!=47 || callback()!=-1 || owner_live!=2 || owner_moves!=1)return 7;
    }
    if(owner_live || owner_constructed!=owner_destroyed)return 8;
    NullableEnvelope original(53);
    NullableEnvelope copied(original);
    if(copied.value.empty || copied.value.value!=53 || nullable_assignments)return 9;
    copied.value=original.value;
    if(nullable_assignments!=1 || copied.value.value!=53)return 10;
    {
        int value=71;
        StoragePointerOwner owner(&value);
        if(read_storage_pointer(owner)!=71 || pointer_destructions)return 11;
        if(owner.value.pointer!=&value)return 12;
    }
    if(pointer_destructions!=1)return 13;
    return 0;
}

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
    return check_initialization_storage();
}
