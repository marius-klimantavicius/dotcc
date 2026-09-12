typedef void (*Handler)(int);
struct Callbacks { Handler receive; Handler unreachable; };
void receive(int value);
void unreachable(int value);
int main(void) {
    struct Callbacks callbacks = { receive, unreachable };
    callbacks.receive(0);
    return 0;
}
