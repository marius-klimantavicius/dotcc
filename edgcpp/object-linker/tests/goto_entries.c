/* Legal C entries into nested control-flow regions. No skipped value is read. */
int jump_into_block(void)
{
    int result = 0;
    goto inside;
    {
        int skipped = 99;
inside:
        result = 7;
    }
    return result;
}

int jump_across_branch(int take_jump)
{
    int result = 0;
    if (take_jump) {
        goto other_branch;
    } else {
other_branch:
        result = 11;
    }
    return result;
}

int jump_into_loop(int remaining)
{
    int sum = 0;
    if (remaining > 0) goto loop_body;
    while (remaining-- > 0) {
loop_body:
        sum += remaining;
        if (sum > 30) break;
    }
    return sum;
}

int jump_into_switch(int value)
{
    if (value == 9) goto switch_body;
    switch (value) {
    case 0:
        goto switch_body;
    case 1:
        return 10;
    case 2:
switch_body:
        return value + 3;
    default:
        return -1;
    }
}

/* An ordinary loop must remain structured despite unrelated difficult gotos. */
int ordinary_loop(int count)
{
    int sum = 0;
    for (int index = 0; index < count; ++index) {
        sum += index;
    }
    return sum;
}

int main(void)
{
    if (jump_into_block() != 7) return 1;
    if (jump_across_branch(0) != 11 || jump_across_branch(1) != 11) return 2;
    if (jump_into_loop(0) != 0 || jump_into_loop(5) != 15) return 3;
    if (jump_into_switch(0) != 3 || jump_into_switch(1) != 10 ||
        jump_into_switch(2) != 5 || jump_into_switch(9) != 12 || jump_into_switch(3) != -1) return 4;
    if (ordinary_loop(10) != 45) return 5;
    return 0;
}
