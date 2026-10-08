/* C++17 sequences assignment RHS before evaluation of the LHS place. Preserve
   both the visible side-effect order and which address is ultimately written. */
static int trace, destination[4], index_value;
static int *place(int digit) { trace = trace * 10 + digit; return &destination[digit]; }
static int value() { trace = trace * 10 + 1; return 41; }

int main()
{
    trace = 0;
    int result = (*place(2) = value());
    if (trace != 12 || result != 41 || destination[2] != 41) return 1;

    trace = 0;
    result = (*place(2) += value());
    if (trace != 12 || result != 82 || destination[2] != 82) return 2;

    trace = 0;
    result = (*place(2) = *place(3) = value());
    if (trace != 132 || result != 41 || destination[2] != 41 || destination[3] != 41) return 3;

    int first = 7, second = 9;
    int *pointer = &first;
    *pointer = (pointer = &second, 42);
    if (first != 7 || second != 42 || pointer != &second) return 4;

    int elements[3] = {7, 8, 9};
    index_value = 0;
    elements[index_value++] = (index_value = 1, 42);
    if (index_value != 2 || elements[0] != 7 || elements[1] != 42 || elements[2] != 9) return 5;
    return 0;
}
