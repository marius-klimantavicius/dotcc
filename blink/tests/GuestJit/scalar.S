/* Pure scalar prefix: every instruction before the final exit syscall is in
 * the allowlist. Snapshot every instruction boundary in the first 128 issues.
 * Invalid/noncanonical LEA results are intentional offsets, never dereferenced.
 * Keep source flags live through LEA, extensions and padding. */
.global _start
.text
_start:
    mov $0x8000000000000001, %rbx
    mov $3, %rsi
    mov $-5, %r12
    mov $0x100000001, %r13
    mov $0x7fffffff, %eax
    add $1, %eax
    lea (%rbx), %rax
    lea 127(%rbx), %rcx
    lea -128(%rbx), %rdx
    lea 0x12345678(%rbx), %r8
    lea -0x12345678(%rbx), %r9
    lea (%rbx,%rsi,1), %r10
    lea -7(%rbx,%rsi,2), %r11
    lea (%r12,%r13,4), %ebp
    lea -0x12345678(%r13,%r12,8), %r15
    lea (%r13), %r14
    lea 123(,%r12,8), %r15
    lea -1(,%rsi,4), %eax
    lea -2147483648, %r15
    lea backward(%rip), %r8
backward:
    lea -1(%rip), %r9
    lea forward(%rip), %rdx
    lea backward(%rip), %edx
    lea (%rbx,%rbx,8), %rbx
    lea (%r12), %r12
    lea -1(%r13), %r13d
    lea -128(%rsp), %rbp
    mov $0x12345678800080ff, %rax
    mov $0x876543218000ff80, %rcx
    movzbl %al, %edi
    movzbw %ah, %bp
    movzbl %ah, %edi
    mov %edi, %r8d
    movsbl %al, %esi
    movsbw %ah, %di
    movsbl %ah, %esi
    movslq %esi, %r9
    movzwl %ax, %r10d
    movzwq %ax, %r11
    movswl %ax, %r12d
    movswq %ax, %r13
    movsbq %r8b, %r14
    movsbl %r9b, %r15d
    movzbl %spl, %edx
    movzbl %dil, %ebp
    movslq %ecx, %r15
    movsbw %ch, %bx
    movzbl %bh, %edi
    movsbl %dh, %esi
    movslq %esi, %r8
    movswq %r13w, %r13
    movsbl %al, %eax
    movswl %cx, %ecx
    movzwl %cx, %ecx
    movzbw %dl, %bx
    /* Ordinary 2..9-byte padding. Avoid Blink's BOFRAM and BINBASE encodings. */
    .byte 0x66,0x90
    .byte 0x0f,0x1f,0x00
    .byte 0x0f,0x1f,0x40,0x00
    .byte 0x0f,0x1f,0x44,0x00,0x00
    .byte 0x66,0x0f,0x1f,0x44,0x00,0x00
    .byte 0x0f,0x1f,0x80,0x00,0x00,0x00,0x00
    .byte 0x0f,0x1f,0x84,0x00,0x00,0x00,0x00,0x00
    .byte 0x66,0x0f,0x1f,0x84,0x00,0x00,0x00,0x00,0x00
    .byte 0xf3,0x0f,0x1e,0xfa
    /* Check offset wrapping, scale, sign extension and 32-bit clearing using
     * only supported comparisons/branches, with both branch outcomes. */
forward:
    cmp $83, %r15
    je failed
    cmp $0x80ff, %r10
    jne failed
    cmp %r10, %r11
    jne failed
    mov $0xffff80ff, %eax
    cmp %rax, %r12
    jne failed
    cmp $-32513, %r13
    jne failed
    cmp $-128, %r14
    jne failed
    mov $0xffffffff8000ff80, %rax
    cmp %rax, %r15
    jne failed
    mov $0x100000001, %r13
    lea -1(%r13), %r13d
    test %r13, %r13
    jne failed
    mov $83, %r9d
    mov $-5, %r12
    lea 123(,%r12,8), %r8
    cmp %r9, %r8
    jne failed
    lea forward(%rip), %r10
    lea backward(%rip), %r11
    cmp %r11, %r10
    jbe failed
    mov $1000, %ecx
again:
    lea -1(%rcx), %ecx
    movzbl %cl, %eax
    .byte 0xf3,0x0f,0x1e,0xfa
    test %ecx, %ecx
    jne again
    /* Explicit interpreted boundaries after the all-native snapshot prefix.
     * LEA ignores the segment base and never accesses guest FS/host FS. */
    leaw (%r12), %bx
    addr32 lea -1(%r13d), %r10d
    .byte 0x64,0x49,0x8d,0x04,0x24
    cmp %r12, %rax
    jne failed
    pxor %xmm0, %xmm0
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.section .note.GNU-stack,"",@progbits
