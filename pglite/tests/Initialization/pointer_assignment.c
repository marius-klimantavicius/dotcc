/* Reduction of original getopt.c: pointer assignment must remain a pointer
 * expression even when a static/global pointer is stored as nint in C#. */
static char *cursor;

int dpg_pointer_assignment(void)
{
    char *arguments[] = {"Z", 0};
    return *(cursor = arguments[0]);
}

int main(void) { return dpg_pointer_assignment() == 90 ? 0 : 1; }
