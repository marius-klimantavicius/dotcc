/* Checked memory fallback first, then a hot native-eligible register loop. */
.global _start
.text
_start:
    sub $16, %rsp
    movq $0, (%rsp)
    mov $10000, %ecx
memory_loop:
    addq $1, (%rsp)
    dec %ecx
    jnz memory_loop
    mov $10000, %ecx
    xor %eax, %eax
register_loop:
    add $1, %rax
    dec %ecx
    jnz register_loop
    cmp $10000, %rax
    jne failed
    cmpq $10000, (%rsp)
    jne failed
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.section .note.GNU-stack,"",@progbits
