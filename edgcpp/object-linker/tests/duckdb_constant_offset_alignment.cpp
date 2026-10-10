struct First { long prefix; };
struct Second { long value; };
struct Derived : First, Second {
    long read() const { return prefix + value; }
};
long downcast_read(Second *part) {
    return static_cast<Derived *>(part)->read();
}
long constant_byte_offset(long *values) {
    return *((long *)((char *)values + 8));
}
int main() {
    Derived value;
    value.prefix = 11;
    value.value = 31;
    long numbers[2] = {7, 42};
    return downcast_read(&value) == 42 && constant_byte_offset(numbers) == 42 ? 0 : 1;
}
