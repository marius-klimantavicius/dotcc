template<class T>
T add(T left, T right) {
    return left + right;
}

template<class T>
T select_value(T value) {
    return add<T>(value, (T)3);
}

extern "C" int fixture() {
    int signed_value = select_value<int>(12);
    unsigned unsigned_value = select_value<unsigned>(20u);
    return signed_value + (int)unsigned_value;
}
