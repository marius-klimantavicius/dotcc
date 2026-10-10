// Unresolved member-template lookup must retain its owning dependent type,
// original member name and ordered arguments without inventing a declaration.
template<class P> struct DuckdbPointerTraits : P {};
template<class P, class U> struct DuckdbSourceMemberTemplateOwner {
    typename DuckdbPointerTraits<P>::template rebind<U>* rebound = nullptr;
    typename P::template apply<U, int>* direct = nullptr;
    typename P::template apply<int, U>* reverse = nullptr;
};
template<class P, class U> struct DuckdbOtherMemberTemplateOwner {
    typename P::template apply<U, int>* other = nullptr;
};
int main() { return 0; }
