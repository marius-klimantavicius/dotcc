/* One valid static x86-64 image with a small explicit TLS startup routine.
 * The ELF loader supplies PT_TLS metadata; this guest performs runtime setup.
 */
.global _start
.section .text
_start:
    lea runtime_tls(%rip), %r12
    mov __tls_template(%rip), %rax
    mov %rax, (%r12)
    movq $0, 8(%r12)
    mov $158, %eax                 /* arch_prctl(ARCH_SET_FS, runtime_tls) */
    mov $0x1002, %edi
    mov %r12, %rsi
    syscall
    test %rax, %rax
    jne failed
    movabs $0x1122334455667788, %rax
    cmp %fs:0, %rax
    jne failed
    cmpq $0, %fs:8
    jne failed
    movq $9, %fs:8
    cmpq $9, %fs:8
    jne failed
    mov %fs:0, %rax
    add %fs:8, %rax
    movabs $0x1122334455667791, %rdx
    cmp %rdx, %rax
    jne failed
    xor %edi, %edi
    mov $60, %eax
.global tls_exit
tls_exit:
    syscall
failed:
    mov $1, %edi
    mov $60, %eax
    syscall

.section .tdata,"awT",@progbits
.balign 8
    .quad 0x1122334455667788
.section .tbss,"awT",@nobits
.balign 8
    .zero 8
.section .bss,"aw",@nobits
.balign 16
.global runtime_tls
runtime_tls:
    .zero 16
.section .note.GNU-stack,"",@progbits
