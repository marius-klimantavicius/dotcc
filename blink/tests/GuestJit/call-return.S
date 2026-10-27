/* Finite ordinary guest call chains. Private stack bytes are backed initially;
 * every cross-page access below is legal and falls back before native issuance. */
.global _start
.text
backward_leaf:
    mov %r12, %r11
    ret
_start:
    mov %rsp, %r15
    lea stack_top(%rip), %rsp
    mov %rsp, %r14
    mov $0x8877665544332211, %r12
    mov $0x13579bdf2468ace0, %r13
    mov $64, %r10d
again:
    call outer
returned_outer:
    mov $0xfedcba9876543210, %rdx
    cmp %rdx, %rax
    jne failed
    cmp %r14, %rsp
    jne failed
    cmp %r13, %r8
    jne failed
    cmp %r12, %r9
    jne failed
    /* Neither a direct call nor C3 may change guest flags. */
    cmp %r12, %r12
    call backward_leaf
    jne failed
    cmp %r12, %r11
    jne failed
    dec %r10d
    jne again
    /* Exact page-end push/pop are checked hits once backing is available. */
    lea arena+4096(%rip), %rsp
    call flag_leaf
returned_end:
    lea arena+4096(%rip), %rdx
    cmp %rdx, %rsp
    jne failed
    lea returned_end(%rip), %rdx
    mov arena+4088(%rip), %rcx
    cmp %rdx, %rcx
    jne failed
    /* Both the call push and return pop span two valid data pages. */
    lea arena+4100(%rip), %rsp
    call flag_leaf
returned_cross:
    lea arena+4100(%rip), %rdx
    cmp %rdx, %rsp
    jne failed
    lea returned_cross(%rip), %rdx
    mov arena+4092(%rip), %rcx
    cmp %rdx, %rcx
    jne failed
    mov %r14, %rsp
    /* Legal prefixed forms stay interpreted rather than widening the allowlist. */
    cmp %r12, %r12
    .byte 0x48, 0xe8
    .long prefixed_leaf - . - 4
    jne failed
    cmp %r14, %rsp
    jne failed
    mov canary_left(%rip), %rdx
    mov $0x123456789abcdef0, %rcx
    cmp %rcx, %rdx
    jne failed
    mov canary_right(%rip), %rdx
    mov $0x0fedcba987654321, %rcx
    cmp %rcx, %rdx
    jne failed
    mov %r15, %rsp
    mov $1, %eax
    mov $1, %edi
    lea message(%rip), %rsi
    mov $message_end-message, %edx
    syscall
    xor %edi, %edi
    jmp quit
outer:
    lea returned_outer(%rip), %rdx
    mov (%rsp), %rcx
    cmp %rdx, %rcx
    jne failed
    lea -8(%r14), %rdx
    cmp %rdx, %rsp
    jne failed
    push %rbp
    mov %rsp, %rbp
    sub $32, %rsp
    mov %r12, (%rsp)
    mov %r13, 8(%rsp)
    call inner
returned_inner:
    mov (%rsp), %r9
    mov 8(%rsp), %r8
    add $32, %rsp
    pop %rbp
    ret
inner:
    lea returned_inner(%rip), %rdx
    mov (%rsp), %rcx
    cmp %rdx, %rcx
    jne failed
    lea -56(%r14), %rdx
    cmp %rdx, %rsp
    jne failed
    push %r12
    push %r13
    pop %rcx
    pop %rdx
    cmp %r13, %rcx
    jne failed
    cmp %r12, %rdx
    jne failed
    mov $0xfedcba9876543210, %rax
    ret
flag_leaf:
    ret
prefixed_leaf:
    .byte 0x48, 0xc3
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
message:
    .ascii "call-return: ok\n"
message_end:
.data
.balign 4096
arena:
    .fill 1024,8,0x12345678
canary_left:
    .quad 0x123456789abcdef0
stack_area:
    .fill 256,8,0x76543210
stack_top:
canary_right:
    .quad 0x0fedcba987654321
.section .note.GNU-stack,"",@progbits
