/* A child keeps executing untouched initial ELF code while the parent legally
 * patches/remaps an isolated initial code page. Nobody executes that page while
 * it is RW. Replacement/anonymous/shared RX pages must remain interpreted. */
.global _start
.text
_start:
    call target
    cmp $1000, %rax
    jne failed
    mov $56, %eax
    mov $0x7d0f00, %edi
    lea child_stack_end(%rip), %rsi
    lea child_tid(%rip), %rdx
    mov %rdx, %r10
    lea child_tls(%rip), %r8
    syscall
    test %rax, %rax
    js failed
    je child
parent_ready:
    cmpl $1, ready(%rip)
    je parent_work
    lea ready(%rip), %rdi
    mov $128, %esi
    xor %edx, %edx
    call wait_word
    jmp parent_ready
parent_work:
    mov $10000, %ecx
    xor %eax, %eax
parent_loop:
    lea 1(%rax), %rax
    dec %ecx
    jne parent_loop
    cmp $10000, %rax
    jne failed
    call target
    cmp $1000, %rax
    jne failed
    /* Mapping mutation must revoke and drain before writable publication. */
    mov $10, %eax
    lea target(%rip), %rdi
    mov $4096, %esi
    mov $3, %edx
    syscall
    test %rax, %rax
    jne failed
    movb $2, target+10(%rip)
    mov $10, %eax
    lea target(%rip), %rdi
    mov $4096, %esi
    mov $5, %edx
    syscall
    test %rax, %rax
    jne failed
    call target
    cmp $2000, %rax
    jne failed
    /* MAP_FIXED at the same VA cannot resurrect the original identity. */
    mov $9, %eax
    lea target(%rip), %rdi
    mov $4096, %esi
    mov $3, %edx
    mov $50, %r10d
    mov $-1, %r8
    xor %r9d, %r9d
    syscall
    lea target(%rip), %r12
    cmp %r12, %rax
    jne failed
    movabs $0x48c031000003e8b9, %rax
    mov %rax, (%r12)
    movabs $0xc3f875c9ff03c083, %rax
    mov %rax, 8(%r12)
    mov $10, %eax
    mov %r12, %rdi
    mov $4096, %esi
    mov $5, %edx
    syscall
    test %rax, %rax
    jne failed
    call target
    cmp $3000, %rax
    jne failed
    /* Ordinary memfd shared RX backing stays on the interpreter. The
     * emulator deliberately rejects anonymous MAP_SHARED mappings. */
    mov $319, %eax
    lea memfd_name(%rip), %rdi
    xor %esi, %esi
    syscall
    test %rax, %rax
    js failed
    mov %rax, %r13
    mov $77, %eax
    mov %r13, %rdi
    mov $4096, %esi
    syscall
    test %rax, %rax
    jne failed
    mov $9, %eax
    xor %edi, %edi
    mov $4096, %esi
    mov $3, %edx
    mov $1, %r10d
    mov %r13, %r8
    xor %r9d, %r9d
    syscall
    cmp $-4095, %rax
    jae failed
    mov %rax, %r12
    movabs $0x48c031000003e8b9, %rax
    mov %rax, (%r12)
    movabs $0xc3f875c9ff04c083, %rax
    mov %rax, 8(%r12)
    mov $10, %eax
    mov %r12, %rdi
    mov $4096, %esi
    mov $5, %edx
    syscall
    test %rax, %rax
    jne failed
    call *%r12
    cmp $4000, %rax
    jne failed
    mov $11, %eax
    mov %r12, %rdi
    mov $4096, %esi
    syscall
    test %rax, %rax
    jne failed
    mov $3, %eax
    mov %r13, %rdi
    syscall
    test %rax, %rax
    jne failed
    movl $1, released(%rip)
parent_join:
    mov child_tid(%rip), %edx
    test %edx, %edx
    je success
    lea child_tid(%rip), %rdi
    xor %esi, %esi
    call wait_word
    jmp parent_join
success:
    xor %edi, %edi
    jmp exit_group
child:
    mov $1, %eax
    xchg %eax, ready(%rip)
    mov $202, %eax
    lea ready(%rip), %rdi
    mov $129, %esi
    mov $1, %edx
    syscall
child_chunk:
    mov $10000, %ecx
    xor %eax, %eax
child_loop:
    lea 3(%rax), %rax
    dec %ecx
    jne child_loop
    cmp $30000, %rax
    jne failed
    cmpl $0, released(%rip)
    je child_chunk
    xor %edi, %edi
    mov $60, %eax
    syscall
    jmp failed
wait_word:
    xor %r10d, %r10d
    xor %r8d, %r8d
    xor %r9d, %r9d
    mov $202, %eax
    syscall
    test %rax, %rax
    je wait_return
    cmp $-11, %rax
    je wait_return
    cmp $-4, %rax
    jne failed
wait_return:
    ret
failed:
    mov $1, %edi
exit_group:
    mov $231, %eax
    syscall
    jmp failed
.balign 4096
target:
    .byte 0xb9,0xe8,3,0,0,0x31,0xc0,0x48,0x83,0xc0,1,0xff,0xc9,0x75,0xf8,0xc3
    .space 4096-16
.section .rodata
memfd_name: .asciz "jit-static-shared"
.section .bss
.balign 16
child_tls: .zero 16
child_tid: .zero 4
ready: .zero 4
released: .zero 4
.balign 16
child_stack: .zero 65536
child_stack_end:
.section .note.GNU-stack,"",@progbits
