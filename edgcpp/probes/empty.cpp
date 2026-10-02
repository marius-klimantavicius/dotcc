struct Empty {};
int main() { Empty e; return sizeof(e) == 1 ? 0 : 1; }
