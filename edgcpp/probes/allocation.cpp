struct Value { int n; Value(int x) : n(x) {} };
int main() {
    Value* p = new Value(42);
    int result = p->n == 42 ? 0 : 1;
    delete p;
    return result;
}
