int pointer_truth(const int *pointer)
{
    if (pointer) return 1;
    return 0;
}

int pointer_not(const int *pointer) { return !pointer; }
int pointer_and(const int *pointer, int flag) { return pointer && flag; }
