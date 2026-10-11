template<class T>
constexpr int selected_size = sizeof(T) + 1;

template<class T>
struct SourceVariableBound {
    char values[selected_size<T>];
};

extern void browse_source_variable(SourceVariableBound<int>*);

int main() {
    return selected_size<int> == sizeof(int) + 1 ? 0 : 1;
}
