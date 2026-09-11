struct extension { unsigned short type; void *data; };
struct hello { struct extension unknown_extensions[3]; int other; };
int main(void)
{
    struct hello ch;
    ch = (struct hello){.unknown_extensions = {{65535}}};
    return ch.unknown_extensions[0].type != 65535 || ch.unknown_extensions[0].data != 0 ||
        ch.unknown_extensions[1].type != 0 || ch.unknown_extensions[2].data != 0 || ch.other != 0;
}
