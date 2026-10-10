// allocator_traits-like nonprototype dependent specialization captured through
// unused class fields. The two argument lists must remain distinct.
template<class Allocator> struct DuckdbSourceTraits {
    using pointer = typename Allocator::pointer;
};
template<class T> struct DuckdbSourceAllocator { using pointer = T*; };
template<class T> struct DuckdbSourceTemplateOwner {
    typename DuckdbSourceTraits<DuckdbSourceAllocator<T>>::pointer value = nullptr;
    typename DuckdbSourceTraits<DuckdbSourceAllocator<const T>>::pointer other = nullptr;
};
template<class T> struct DuckdbOtherSourceTemplateOwner {
    typename DuckdbSourceTraits<DuckdbSourceAllocator<T>>::pointer value = nullptr;
};
int main() { return 0; }
