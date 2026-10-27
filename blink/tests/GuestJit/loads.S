/* Initial private .data hits and a legal cross-page miss. No fault injection.
 * Flags survive MOV/MOVZX loads. The miss must issue in the interpreter once. */
.global _start
.text
_start:
    lea data(%rip), %rbx
    mov $1, %ecx
    mov $0x7fffffff, %eax
    add $1, %eax
    mov (%rbx), %r8
    mov 4(%rbx), %r9d
    movzbl (%rbx), %r10d
    movzbq 1(%rbx), %r11
    movzwl 2(%rbx), %r12d
    movzwq 4(%rbx), %r13
    mov (%rbx,%rcx,1), %r14
    mov 3(%rbx,%rcx,2), %r15d
    movzwl 1(%rbx,%rcx,4), %edi
    movzbl -1(%rbx,%rcx,8), %ebp
    mov data+8(%rip), %rax
    movzbl data+3(%rip), %edx
    movzwq data+2(%rip), %rsi
    mov (%rbx), %rbx
    lea data(%rip), %rbx
    mov (%rbx,%rcx,8), %ecx
    mov 4093(%rbx), %rdx
    movzwl 4095(%rbx), %eax
    movzbl 4095(%rbx), %eax
    mov $1000, %ecx
again:
    mov (%rbx), %r8
    mov 4(%rbx), %r9d
    movzbl 1(%rbx), %r10d
    movzwq 2(%rbx), %r11
    dec %ecx
    jne again
    mov $0x8877665544332211, %rax
    cmp %rax, %r8
    jne failed
    mov $0x88776655, %eax
    cmp %rax, %r9
    jne failed
    cmp $0x22, %r10
    jne failed
    cmp $0x4433, %r11
    jne failed
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.data
.balign 4096
data:
    .quad 0x8877665544332211,0x0123456789abcdef
    .space 4096-16
    .quad 0xfedcba9876543210
    .space 4096-8
.section .note.GNU-stack,"",@progbits
