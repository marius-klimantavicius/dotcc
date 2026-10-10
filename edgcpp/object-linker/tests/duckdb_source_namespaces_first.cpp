#define NAMESPACE_EXTENT 1
#include "duckdb_source_namespaces.hpp"
extern "C" int second();
extern "C" int first() {
    original_public::Hidden value{};
    original_public::Holder<int> holder{};
    value.values[0] = holder.value + 1;
    return original_public::Hidden::call(value.values[0]) - 1;
}
int main() { return first() + second() == 20 ? 0 : 1; }
