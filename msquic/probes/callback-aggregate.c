/* C function designator must decay in an aggregate initializer. */
typedef void (*Handler)(int);
struct Callbacks { Handler receive; Handler unreachable; };
void receive(int value) { (void)value; }
void unreachable(int value) { (void)value; }
int main(void) {
    struct Callbacks callbacks = { receive, unreachable };
    callbacks.receive(0);
    return 0;
}
