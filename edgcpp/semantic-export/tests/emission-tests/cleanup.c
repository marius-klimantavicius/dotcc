void cleanup(int *x) { *x = 0; }
int cleanup_case(void)
{
    int x __attribute__((cleanup(cleanup))) = 7;
    return x;
}
