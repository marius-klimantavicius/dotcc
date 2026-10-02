struct Value { char c; long double n; };
int main() {
    Value v{};
    long actual_offset = reinterpret_cast<char*>(&v.n) - reinterpret_cast<char*>(&v);
    return sizeof(Value) == 32 && sizeof(v.n) == 16 && actual_offset == 16 ? 0 : 1;
}
