template<class T> struct QueryConstants {
    char size_type[sizeof(T)];
    char size_expression[sizeof(*(T*)0)];
    char alignment_type[alignof(T)];
    char alignment_expression[__alignof__(*(T*)0)];
};

extern void browse_queries(QueryConstants<int>*);

constexpr unsigned long folded_size = sizeof(int);
constexpr unsigned long folded_alignment = alignof(int);

int main() {
    return folded_size == sizeof(int) && folded_alignment == alignof(int) ? 0 : 1;
}
