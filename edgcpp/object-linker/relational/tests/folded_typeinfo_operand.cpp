#include <typeinfo>

// EDG folds the nonpolymorphic operand to its selected static typeinfo value.

int operand;

const std::type_info* source_operand_address() {
    return &typeid(operand);
}

int main() {
    return source_operand_address() == nullptr;
}
