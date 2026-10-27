/* Ordinary private-memory witness for entry-value recipes. The hot prologue
 * copies RDI into RBX after changing RSP/RBP; old RBX names another page.
 * Loaded-pointer dependencies remain interpreter/guard continuations. */
.global _start
.text
_start:
    mov %rsp, %r14
    lea stack_top(%rip), %rsp
    lea object(%rip), %rdi
    lea other(%rip), %rbx
    mov $1000, %r13d
again:
    mov $1, %esi
    call hot
    lea other(%rip), %rbx
    dec %r13d
    jne again
    cmp $0x1234, %eax
    jne failed
    lea object(%rip), %rdi
    mov (%rdi), %rax
    mov (%rax), %rdx
    cmp $0x1234, %edx
    jne failed
    /* Partial aliases and 32-bit zero extension feed a valid address. */
    mov $-1, %rcx
    mov $0, %ecx
    mov $1, %cl
    mov $0, %ch
    lea value-8(%rip), %rax
    lea (%rax,%rcx,8), %rax
    mov (%rax), %rdx
    cmp $0x1234, %edx
    jne failed
    mov %r14, %rsp
    xor %edi, %edi
    jmp quit
hot:
    push %rbp
    push %r15
    push %rbx
    sub $16, %rsp
    lea 32(%rsp), %rbp
    mov %rdi, %rbx
    mov %rdx, %r15
    test %esi, %esi
    jl failed
    mov 16(%rbx), %edi
    cmp %edi, %esi
    jae failed
    mov 8(%rbx), %rax
    mov %esi, %esi
    add %rax, %rsi
    mov %rsi, -24(%rbp)
    mov %edi, %esi
    add %rax, %rsi
    lea -24(%rbp), %rdi
    call leaf
    add $16, %rsp
    pop %rbx
    pop %r15
    pop %rbp
    lea object(%rip), %rdi
    ret
leaf:
    mov $0x1234, %eax
    ret
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.data
.balign 4096
object:
    .quad value, 0x100000
    .long 4, 0
    .space 4096-24
other:
    .space 4096
value:
    .quad 0x1234
    .space 4096-8
stack:
    .fill 512,8,0x10203040
stack_top:
    .space 64
.section .note.GNU-stack,"",@progbits
