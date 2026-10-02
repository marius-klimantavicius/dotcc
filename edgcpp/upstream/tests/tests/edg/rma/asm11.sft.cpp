//options_all:-r -x -tused
//options: --strict;cn

asm int mulword(a,b)
{
%reg a %reg b
    muls a,b
}

