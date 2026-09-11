int main(void)
{
    const char *p = "hello", *const end = p + 5;
    while (p != end) ++p;
    return *p;
}
