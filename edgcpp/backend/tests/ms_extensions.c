/* Named anonymous struct members used by MsQuic's portable C core. */
struct Base { unsigned kind; void *context; };
typedef struct Payload { unsigned short size; unsigned char *data; } Payload;
struct Record {
    struct Base;
    Payload;
    unsigned flags;
};
int main(void) {
    unsigned char data[3] = {7, 8, 9};
    struct Record value = {0};
    value.kind = 11;
    value.context = &value;
    value.size = 3;
    value.data = data;
    value.flags = 19;
    struct Base *base = (struct Base *)&value;
    return !(base->kind == 11 && base->context == &value &&
             value.size == 3 && value.data[2] == 9 && value.flags == 19);
}
