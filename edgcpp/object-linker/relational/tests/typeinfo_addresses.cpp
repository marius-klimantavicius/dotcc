#include <typeinfo>

const std::type_info* void_address = &typeid(void);
const std::type_info* int_address = &typeid(int);
const std::type_info* double_address = &typeid(double);
const std::type_info* pointer_address = &typeid(int*);
using selected_integer = const int;
const std::type_info* qualified_int_address = &typeid(selected_integer);

const std::type_info* const type_addresses[] = {
    &typeid(void), &typeid(int), &typeid(double), &typeid(int*)
};

struct TypePolicy {
    bool empty;
    const std::type_info* info;
};

const TypePolicy* empty_policy() {
    static constexpr TypePolicy policy = {true, &typeid(void)};
    return &policy;
}

const std::type_info* local_void_address() {
    static const std::type_info* value = &typeid(void);
    return value;
}

const std::type_info* direct_void_address() {
    return &typeid(void);
}

int main() {
    return direct_void_address() == void_address ? 0 : 1;
}
