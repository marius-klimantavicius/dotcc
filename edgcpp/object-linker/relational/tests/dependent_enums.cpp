template<class T> struct EnumBound {
    enum Limits { extent = sizeof(T) + 1, alignment = alignof(T) };
    char values[extent];
};

extern void browse_enum(EnumBound<int>*);

int concrete_value(EnumBound<int>::Limits value) {
    return (int)value;
}

int main() {
    if (concrete_value(EnumBound<int>::extent) != sizeof(int) + 1) return 1;
    if (concrete_value(EnumBound<int>::alignment) != alignof(int)) return 2;
    return 0;
}
