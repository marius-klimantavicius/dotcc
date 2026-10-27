/* Finite private stack/store witness. All addresses are valid guest memory.
 * Cross-page accesses exercise guard fallback, without guest fault injection. */
.global _start
.text
_start:
    push %rsp
    pop %r11
    cmp %rsp, %r11
    jne failed
    mov %rsp, %r15
    lea stack_top(%rip), %rsp
    mov %rsp, %r10
    push %rbp
    mov %rsp, %rbp
    sub $32, %rsp
    mov $0x8877665544332211, %rax
    mov %rax, (%rsp)
    mov %eax, 8(%rsp)
    mov (%rsp), %r8
    cmp %rax, %r8
    jne failed
    mov 8(%rsp), %r9d
    cmp %eax, %r9d
    jne failed
    add $32, %rsp
    pop %rbp
    push %rsp
    pop %r12
    cmp %r10, %r12
    jne failed
    lea -64(%rsp), %rax
    push %rax
    pop %rsp
    cmp %rax, %rsp
    jne failed
    mov %r10, %rsp
    push %r8
    push %r9
    pop %r13
    pop %r14
    cmp %r8, %r14
    jne failed
    cmp %r9, %r13
    jne failed
    lea arena(%rip), %rbx
    mov $1, %ecx
    mov $0xaabbccdd8899aabb, %rax
    mov %rax, (%rbx)
    mov %eax, 8(%rbx)
    mov %rax, 8(%rbx,%rcx,8)
    mov %rax, arena+24(%rip)
    mov %rsp, 32(%rbx)
    mov %rbx, 40(%rbx)
    mov %rax, (%rsp)
    mov %rsp, 8(%rsp)
    mov 8(%rsp), %rdx
    cmp %rsp, %rdx
    jne failed
    mov 8(%rbx), %rdx
    mov $0x112233448899aabb, %rsi
    cmp %rsi, %rdx
    jne failed
    /* Check both adjacent canaries after an exact four-byte RIP store. */
    mov %eax, store_slot(%rip)
    mov canary_left(%rip), %rdx
    mov $0x13579bdf2468ace0, %rsi
    cmp %rsi, %rdx
    jne failed
    mov store_slot(%rip), %rdx
    mov $0x7abedede8899aabb, %rsi
    cmp %rsi, %rdx
    jne failed
    mov canary_right(%rip), %rdx
    mov $0xfedcba9876543210, %rsi
    cmp %rsi, %rdx
    jne failed
    mov %eax, arena+48(%rip)
    mov arena+48(%rip), %edx
    cmp %eax, %edx
    jne failed
    mov %r14, 56(%rbx)
    mov 56(%rbx), %rdx
    cmp %r14, %rdx
    jne failed
    mov %r13d, 64(%rbx)
    mov 64(%rbx), %edx
    cmp %r13d, %edx
    jne failed
    mov 8(%rbx,%rcx,8), %r12
    cmp %rax, %r12
    jne failed
    mov arena+24(%rip), %r13
    cmp %rax, %r13
    jne failed
    mov 32(%rbx), %rdx
    cmp %rsp, %rdx
    jne failed
    mov 40(%rbx), %rdx
    cmp %rbx, %rdx
    jne failed
    mov %eax, 4092(%rbx)
    mov 4092(%rbx), %edx
    cmp %eax, %edx
    jne failed
    mov %rax, 4093(%rbx)
    mov 4093(%rbx), %rdx
    cmp %rax, %rdx
    jne failed
    lea 4101(%rbx), %rsp
    push %rax
    pop %rdx
    cmp %rax, %rdx
    jne failed
    lea 4096(%rbx), %rsp
    push %rax
    pop %rdx
    cmp %rax, %rdx
    jne failed
    mov %r10, %rsp
    /* Operand-size16 stays interpreted, with valid private stack operands. */
    .byte 0x66,0x50
    .byte 0x66,0x59
    cmp %ax, %cx
    jne failed
    mov $1000, %ecx
again:
    push %rbp
    mov %rsp, %rbp
    sub $16, %rsp
    mov %rax, (%rsp)
    mov (%rsp), %r8
    mov %eax, 8(%rsp)
    mov 8(%rsp), %r9d
    add $16, %rsp
    pop %rbp
    push %r8
    pop %r12
    dec %ecx
    jne again
    cmp %rax, %r12
    jne failed
    cmp %eax, %r9d
    jne failed
    cmp %r10, %rsp
    jne failed
    mov %r15, %rsp
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.data
.balign 4096
arena:
    .quad 0,0x11223344ffffffff
    .space 4096-16
    .quad 0
    .space 4096-8
canary_left:
    .quad 0x13579bdf2468ace0
store_slot:
    .long 0xffffffff,0x7abedede
canary_right:
    .quad 0xfedcba9876543210
    /* Backed initial private bytes, rather than a new runtime mapping. */
stack_area:
    .fill 256,8,0x12345678
stack_top:
    .space 64
.section .note.GNU-stack,"",@progbits
