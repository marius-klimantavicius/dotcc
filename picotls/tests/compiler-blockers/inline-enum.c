struct state { enum phase { NONE, READY = 7, DONE } phase; };
int main(void)
{
    struct state s = {READY};
    enum { FULL, PSK, PSK_DHE } mode = PSK_DHE;
    return s.phase + mode - 9;
}
