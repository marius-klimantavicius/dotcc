/* Dynamic copy, exact same-object alias copy, and partial local aggregate init.
   This application source is exported; the oracle harness remains separate. */
struct Aggregate {
    short left;
    unsigned char tag;
    int tail;
};

static struct Aggregate source = {-7, 4, 11};

static int mutate_copy(struct Aggregate *input) {
    struct Aggregate copy = *input;
    copy.left += 3;
    struct Aggregate output;
    output = copy;
    struct Aggregate *same = &output;
    output = *same;
    *input = output;
    return output.left + output.tag + output.tail;
}

int fixture(void) {
    struct Aggregate partial[3] = {{2, 5, 9}, {7}};
    int result = mutate_copy(&source);
    partial[2] = source;
    return result + partial[0].left + partial[0].tag + partial[0].tail
        + partial[1].left + partial[1].tag + partial[1].tail
        + partial[2].left + partial[2].tag + partial[2].tail;
}
