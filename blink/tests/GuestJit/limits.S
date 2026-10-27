/* All initial GPRs/SIMD values and the stack are loader-defined; no syscalls or
 * address-dependent arithmetic in this prefix, suitable for exact snapshots. */
.global _start
.text
_start:
    mov $0x7fffffff, %eax
    mov $0x123456789abcdef, %r8
    mov $0x55, %bh
    add $1, %eax
    mov $0xffff, %dx
    add $1, %dx
    sub $1, %al
    inc %r8
    dec %r8
    xor %ecx, %ecx
    cmp $1, %ecx
    jb again
    nop
again:
    add $1, %ecx
    cmp $10000000, %ecx
    jne again
    xor %edi, %edi
    mov $60, %eax
    syscall
.section .note.GNU-stack,"",@progbits
