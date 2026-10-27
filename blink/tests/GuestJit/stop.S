/* Ordinary finite CPU work; the host supplies a bounded deadline/cancellation. */
.global _start
.text
_start:
    mov $1000000000, %ecx
    xor %eax, %eax
again:
    add $1, %rax
    sub $1, %ecx
    jnz again
    xor %edi, %edi
    mov $60, %eax
    syscall
.section .note.GNU-stack,"",@progbits
