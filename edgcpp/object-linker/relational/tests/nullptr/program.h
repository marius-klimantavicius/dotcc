using Null = decltype(nullptr);
struct Holder { Null value; int *pointer; };
Null make_null();
int accept_null(Null value);
int *as_pointer(Null value);
int select(int *value);
int select(Null value);
template<Null Value = nullptr> int null_template() { return Value == nullptr ? 7 : 9; }
Null with_effect(int *counter);
