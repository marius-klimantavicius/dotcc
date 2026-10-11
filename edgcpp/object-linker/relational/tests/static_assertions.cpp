template<class T>
int selected_width() {
    static_assert(sizeof(T) == 4, "templated_selected");
    static_assert(sizeof(T) > 0);
    return sizeof(T);
}

int main() {
    static_assert(1 + 2 == 3, "local_selected");
    static_assert(sizeof(int) == 4);
    static_assert(true, "");
    return selected_width<int>() == 4 ? 0 : 1;
}
