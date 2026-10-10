#ifndef DUCKDB_SOURCE_NAMESPACES_HPP
#define DUCKDB_SOURCE_NAMESPACES_HPP
namespace original_public {
namespace {
enum class Choice { selected = 3 };
struct Hidden {
    int values[NAMESPACE_EXTENT];
    Choice choice;
    static int call(int value) { return value + (int)sizeof(Hidden); }
};
}
// This reopens the same original unnamed namespace; named descendants inherit
// its translation-unit identity without acquiring fabricated source names.
namespace {
namespace named_child {
template<class T> using Pointer = T*;
struct Other { static int call(int value) { return value; } };
}
template<class T> concept Callable = requires(T value) {
    Hidden::call(value);
    named_child::Other::call(value);
    sizeof(named_child::Pointer<T>);
};
template<Callable T> struct Holder { T value = {}; };
}
}
#endif
