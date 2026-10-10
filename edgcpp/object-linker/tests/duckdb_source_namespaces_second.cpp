#define NAMESPACE_EXTENT 2
#include "duckdb_source_namespaces.hpp"
extern "C" int second() {
    original_public::Hidden value{};
    original_public::Holder<int> holder{};
    value.values[0] = holder.value + 2;
    return original_public::Hidden::call(value.values[0]) - 2;
}
