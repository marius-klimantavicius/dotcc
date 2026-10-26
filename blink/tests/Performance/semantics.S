/* Ordinary finite CPU/memory smoke for fresh full interpreter closures.
 * Includes the scalar comparison-mask regression relevant to Kestrel, flags,
 * and unaligned vector memory operations crossing a guest page boundary. */
.global _start
.section .text
_start:
    mov $0x7fffffff, %eax
    add $1, %eax
    jno failed
    jns failed
    jz failed
    jc failed
    mov $-1, %eax
    add $1, %eax
    jnc failed
    jnz failed
    jo failed
    mov $0x3f800000, %eax
    movd %eax, %xmm0
    cmpss $7, %xmm0, %xmm0
    movd %xmm0, %eax
    cmp $-1, %eax
    jne failed
    mov $0x7fc00000, %eax
    movd %eax, %xmm0
    cmpss $7, %xmm0, %xmm0
    movd %xmm0, %eax
    test %eax, %eax
    jne failed
    movdqu crossing(%rip), %xmm0
    movdqu %xmm0, crossing+32(%rip)
    movdqu crossing+32(%rip), %xmm1
    pcmpeqb %xmm1, %xmm0
    pmovmskb %xmm0, %eax
    cmp $65535, %eax
    jne failed
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.section .data
.balign 4096
    .zero 4090
crossing:
    .quad 0x0102030405060708, 0x1112131415161718
    .zero 64
.section .note.GNU-stack,"",@progbits
