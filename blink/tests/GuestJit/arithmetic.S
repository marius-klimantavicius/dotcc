/* Valid finite scalar ISA/control-flow fixture. Memory/SIMD/syscalls deliberately
 * cross hybrid boundaries; all results are checked in the guest itself. */
.global _start
.text
_start:
    mov $0x7fffffff, %eax
    add $1, %eax
    jno failed
    jns failed
    jc failed
    mov $-1, %rax
    add $1, %rax
    jnc failed
    jnz failed
    mov $0x7f, %al
    add $1, %al
    jno failed
    jns failed
    mov $0x7fff, %ax
    add $1, %ax
    jno failed
    jns failed
    mov $0x7fff, %ax
    sub $-1, %ax
    jno failed
    mov $0x1122334455667788, %r8
    mov $0xaa, %r8b
    mov $0xbbcc, %r8w
    mov $0x5566bbcc, %eax
    cmp %eax, %r8d
    jne failed
    mov $0x12345678, %r8d
    mov $0x12345678, %r9
    cmp %r8, %r9
    jne failed
    mov $0x1234, %eax
    mov $0x56, %ah
    cmp $0x5634, %eax
    jne failed
    mov %ah, %bl
    cmp $0x56, %bl
    jne failed
    mov $0, %eax
    sub $1, %eax
    inc %eax
    jnc failed
    jnz failed
    dec %eax
    jnc failed
    cmp $-1, %eax
    jne failed
    xor %eax, %eax
    jp even
    jmp failed
even:
    mov $1, %al
    test %al, %al
    jnp odd
    jmp failed
odd:
    mov $1000, %ecx
    xor %edx, %edx
again:
    add $1, %edx
    sub $1, %ecx
    jnz again
    cmp $1000, %edx
    jne failed
    sub $16, %rsp
    mov %rdx, (%rsp)
    pxor %xmm0, %xmm0
    addq $1, (%rsp)
    mov (%rsp), %r10
    cmp $1001, %r10
    jne failed
    add $16, %rsp
    mov $39, %eax
    syscall
    test %eax, %eax
    jle failed
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.section .note.GNU-stack,"",@progbits
