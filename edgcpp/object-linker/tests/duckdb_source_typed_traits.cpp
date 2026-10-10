// Preserve unevaluated compiler traits as typed semantic expressions.  The
// ordered types and their original template binders are part of identity.
template<bool Selected> struct DuckdbSourceTraitResult {};
template<unsigned long Size> struct DuckdbSourceSizeResult {};
template<class T, class U> struct DuckdbSourceTypedExpressionOwner {
    DuckdbSourceTraitResult<__is_empty(T)>* empty = nullptr;
    DuckdbSourceTraitResult<__is_base_of(T, U)>* base = nullptr;
    DuckdbSourceTraitResult<__is_base_of(U, T)>* reverse = nullptr;
    DuckdbSourceTraitResult<__is_constructible(T, U, int)>* construct = nullptr;
    DuckdbSourceSizeResult<sizeof(T)>* size = nullptr;
    DuckdbSourceSizeResult<alignof(T)>* alignment = nullptr;
    DuckdbSourceSizeResult<__alignof__(T::value)>* extended_alignment = nullptr;
    DuckdbSourceSizeResult<sizeof(T::value)>* expression_size = nullptr;
    int values[sizeof(T) + alignof(U)] = {};
};
template<class T> struct DuckdbOtherTypedExpressionOwner {
    DuckdbSourceTraitResult<__is_empty(T)>* distinct_owner = nullptr;
};
struct DuckdbEmptyType {};
struct DuckdbDerivedType : DuckdbEmptyType {};
struct DuckdbConstructedType { DuckdbConstructedType(DuckdbEmptyType, int) {} };
static_assert(__is_empty(DuckdbEmptyType));
static_assert(__is_base_of(DuckdbEmptyType, DuckdbDerivedType));
static_assert(!__is_base_of(DuckdbDerivedType, DuckdbEmptyType));
static_assert(__is_constructible(DuckdbConstructedType, DuckdbEmptyType, int));
int main() { return 0; }
