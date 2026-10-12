int trace = 0;

const char* data_argument(const char* value) {
    trace = trace * 10 + 1;
    return value;
}

int size_argument(int value) {
    trace = trace * 10 + 2;
    return value;
}

struct View {
    View* self;
    const char* data;
    int size;
    View(const char* selected, int count) : self(this), data(selected), size(count) {
        trace = trace * 10 + 3;
        if (count == 9) throw count;
    }
};

struct Holder {
    View view;
};

int main() {
    Holder holder{{data_argument(nullptr), size_argument(7)}};
    if (trace != 123 || holder.view.self != &holder.view || holder.view.data != nullptr || holder.view.size != 7) return 1;
    trace = 0;
    try {
        Holder failed{{data_argument(nullptr), size_argument(9)}};
        return 2;
    } catch (int code) {
        if (code != 9 || trace != 123) return 3;
    }
    return 0;
}
