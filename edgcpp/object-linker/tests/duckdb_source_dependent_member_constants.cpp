// Trait<T>::value remains an unresolved, typed semantic member until the
// owning template is instantiated. Equal spellings under different owners
// must not collapse template identities.
template<class T> struct DuckdbSourceTrait { static constexpr bool value = true; };
template<class T> struct DuckdbOtherSourceTrait { static constexpr bool value = false; };
template<bool Selected> struct DuckdbSourceTag {};
template<class T> struct DuckdbSourceMemberConstantOwner {
    DuckdbSourceTag<DuckdbSourceTrait<T>::value>* first = nullptr;
    DuckdbSourceTag<DuckdbOtherSourceTrait<T>::value>* second = nullptr;
    int values[T::extent] = {};
};
int main() { return 0; }
