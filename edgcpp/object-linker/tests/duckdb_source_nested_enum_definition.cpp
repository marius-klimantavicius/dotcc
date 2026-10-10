// Node has an original declaration and a dependent view within its template.
template<class T> struct DuckdbNestedTypeOwner {
    struct Node { Node* next; T value; };
    enum class State { empty = 0, ready = 1 };
    using Value = T;
    Node* head = nullptr;
    Node* tail = nullptr;
    State status = State::empty;
    Value* payload = nullptr;
};
template<class T> struct DuckdbOtherNestedTypeOwner {
    struct Node { T* value; };
    Node* head = nullptr;
};
int main() { return 0; }
