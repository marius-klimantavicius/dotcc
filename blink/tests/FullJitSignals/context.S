/* Deliberate guest #UD, #XM and aligned-SSE #GP probes. */
.global _start, raw_syscall4, try_instruction, fault_site, fallback_site, restorer
.section .text
_start:
    and $-16, %rsp
    call guest_main
    mov %eax, %edi
    mov $231, %eax
    syscall
    jmp .
raw_syscall4:
    mov %rdi, %rax
    mov %rsi, %rdi
    mov %rdx, %rsi
    mov %rcx, %rdx
    mov %r8, %r10
    syscall
    ret
try_instruction:
    push %r12
    mov %rsp, saved_sp(%rip)
    ldmxcsr initial_mxcsr(%rip)
    xor %ecx, %ecx
    mov $0x1122334455667788, %rax
    mov $0x8877665544332211, %r12
    stc
#ifdef PROBE_GP
    movdqu gp_initial(%rip), %xmm0
#elif defined(PROBE_SIMD)
    movss float_one(%rip), %xmm0
    movss float_zero(%rip), %xmm1
#endif
fault_site:
#ifdef PROBE_GP
    movaps gp_unaligned(%rip), %xmm0 /* #GP before destination effects */
#elif defined(PROBE_SIMD)
    divss %xmm1, %xmm0            /* guest-unmasked #XM; host must survive */
#elif defined(PROBE_XGETBV)
    .byte 0x0f, 0x01, 0xd0         /* XGETBV: absent in the guest CPU profile */
#else
    ud2
#endif
    mov $28, %edi                 /* no automatic skip or fall-through */
    mov $231, %eax
    syscall
    jmp .
fallback_site:
    pushfq
    popq returned_flags(%rip)
    stmxcsr returned_mxcsr(%rip)
    pop %r12
    ret
restorer:
    mov $15, %eax
    syscall
    mov $29, %edi
    mov $231, %eax
    syscall
    jmp .
.section .rodata
.balign 4
initial_mxcsr:
#ifdef PROBE_SIMD
    .long 0x1d80                 /* unmask guest divide by zero */
float_one: .long 0x3f800000
float_zero: .long 0
#else
    .long 0x1f80
#endif
#ifdef PROBE_GP
.balign 16
gp_initial: .long 0x11223344, 0x55667788, 0x99aabbcc, 0xddeeff00
.balign 16
    .byte 0
gp_unaligned: .long 1, 2, 3, 4
#endif
.section .bss
.balign 8
.global saved_sp, returned_flags, returned_mxcsr
saved_sp: .zero 8
returned_flags: .zero 8
returned_mxcsr: .zero 4
.section .note.GNU-stack,"",@progbits
