// libcxx variant::__farray keeps a default member initializer for an array
// whose bound is a non-type template parameter. Preserve the typed bound;
// an unsized array and a dependent array are different semantic types.
template<class T, unsigned long Size> struct DuckdbSourceDependentArray {
    T direct[Size] = {};
    T expression[Size + 1] = {};
    T nested[2][Size] = {};
    int (*unsized)[] = nullptr;
};
extern int duckdb_source_unsized_array[];
int main() { return 0; }
