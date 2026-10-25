.global _start,spawn
.text
_start:
    and $-16,%rsp
    call guestmain
    ud2
/* SysV arguments: stack top, clear-child-TID word, ordinary C entry. */
spawn:
    sub $8,%rdi
    mov %rdx,(%rdi)
    mov %rsi,%rdx
    mov %rsi,%r10
    mov %rdi,%rsi
    mov $0x310f00,%edi
    xor %r8d,%r8d
    mov $56,%eax
    syscall
    test %rax,%rax
    jnz parent
    pop %rax
    call *%rax
    xor %edi,%edi
    mov $60,%eax
    syscall
    ud2
parent:
    ret
.section .note.GNU-stack,"",@progbits
