// ICU char16ptr.h uses a register-or-memory input for its aliasing barrier.
static int evaluations;
static int destructions;
static unsigned short **select_slot(unsigned short **slot) {
    ++evaluations;
    return slot;
}
struct View {
    unsigned short *p_;
    explicit View(unsigned short *p) : p_(p) {
        asm volatile("" : : "rm"(p_) : "memory");
    }
    ~View() {
        asm volatile("" : : "mr"(p_) : "memory");
        ++destructions;
    }
    char16_t *get() const {
        unsigned short *p = p_;
        asm volatile("" : : "rm"(p) : "memory");
        return reinterpret_cast<char16_t *>(p);
    }
};
int main() {
    unsigned short text[2] = {65, 0};
    unsigned short *p = text;
    {
        View view(p);
        asm volatile("" : : "rm"(*select_slot(&p)) : "memory");
        view.get()[0] = 66;
        if (evaluations != 1 || text[0] != 66 || destructions != 0) return 1;
    }
    return destructions != 1 || p != text;
}
