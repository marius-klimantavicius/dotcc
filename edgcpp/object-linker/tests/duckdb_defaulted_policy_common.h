extern int policy_failure;
template<class T> struct PolicyBase {
    T value;
    PolicyBase() noexcept : value(7) {}
};
template<class T> struct PolicyDerived : PolicyBase<T> {
    PolicyDerived() = default;
    explicit PolicyDerived(int) noexcept {}
};
template<class T> struct PolicyThrowingBase {
    T value;
    PolicyThrowingBase() noexcept(false) : value(9) { if (policy_failure) throw 42; }
};
template<class T> struct PolicyMayThrow : PolicyThrowingBase<T> {
    PolicyMayThrow() = default;
};
template<class T> struct PolicyExplicit : PolicyBase<T> {
    PolicyExplicit() noexcept(false) = default;
};
struct PolicyWithoutDefaultMember {};
template<class T> struct PolicyUnusedDefault {
    int value = T::missing_default_member;
    PolicyUnusedDefault() = default;
    explicit PolicyUnusedDefault(int v) : value(v) {}
};
