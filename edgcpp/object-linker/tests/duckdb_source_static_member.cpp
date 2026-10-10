namespace StaticMemberSource {
template<unsigned long Size> struct Storage { unsigned char value[Size]; };
template<class T> struct Owner {
    static const unsigned long extent = sizeof(T);
    using StorageType = Storage<extent>;
    StorageType value = {};
};
template<class T> struct OtherOwner {
    static const unsigned long extent = sizeof(T) + 1;
    using StorageType = Storage<extent>;
    StorageType value = {};
};
}
int main() { return 0; }
