// Stock's constructor-initializer no-effect pass is separate from its
// recursive lifetime pass. Preserve the original obligation and this exact
// elimination even though no runtime cleanup action remains.
struct DuckdbNoEffectBase {
    ~DuckdbNoEffectBase() {}
};
struct DuckdbNoEffectOwner : DuckdbNoEffectBase {
    DuckdbNoEffectOwner() {}
};
int main() {
    DuckdbNoEffectOwner owner;
    return 0;
}
