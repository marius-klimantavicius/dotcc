/* Valid static guests. An extra argv selects an ordinary pending operation.
 * Native witnesses run only the finite, no-extra-argument controls. */
.global _start
.section .text
_start:
    mov (%rsp), %r12
#if defined(CASE_CPU)
    xor %eax, %eax
    cmp $1, %r12
    jne cpu_wait
    mov $65536, %ecx
cpu_count:
    add $1, %rax
    dec %ecx
    jne cpu_count
    cmp $65536, %rax
    jne failed
    jmp success
cpu_wait:
    add $1, %rax
    jmp cpu_wait
#elif defined(CASE_POLL)
    xor %edi, %edi
    xor %esi, %esi
    mov $5, %edx
    cmp $1, %r12
    je poll_call
    mov $-1, %edx
poll_call:
    mov $7, %eax
    syscall
    test %rax, %rax
    jne failed
    jmp success
#elif defined(CASE_SLEEP)
    cmp $1, %r12
    je sleep_call
    movq $30, request(%rip)
    movq $0, request+8(%rip)
sleep_call:
    lea request(%rip), %rdi
    lea remaining(%rip), %rsi
    mov $35, %eax
    syscall
    test %rax, %rax
    jne failed
    jmp success
#elif defined(CASE_PIPE)
    lea pipefds(%rip), %rdi
    mov $22, %eax
    syscall
    test %rax, %rax
    jne failed
    cmp $1, %r12
    jne pipe_read
    mov pipefds+4(%rip), %edi
    lea payload(%rip), %rsi
    mov $1, %edx
    mov $1, %eax
    syscall
    cmp $1, %rax
    jne failed
pipe_read:
    mov pipefds(%rip), %edi
    lea received(%rip), %rsi
    mov $1, %edx
    xor %eax, %eax
    syscall
    cmp $1, %rax
    jne failed
    cmpb $0x5a, received(%rip)
    jne failed
    mov pipefds(%rip), %edi
    mov $3, %eax
    syscall
    test %rax, %rax
    jne failed
    mov pipefds+4(%rip), %edi
    mov $3, %eax
    syscall
    test %rax, %rax
    jne failed
#else
#error Select exactly one reviewed fixture
#endif
success:
    mov $1, %edi
    lea marker(%rip), %rsi
    mov $(marker_end-marker), %edx
    mov $1, %eax
    syscall
    cmp $(marker_end-marker), %rax
    jne failed
    xor %edi, %edi
    mov $60, %eax
    syscall
failed:
    mov $1, %edi
    mov $60, %eax
    syscall
.section .rodata
marker:
#if defined(CASE_CPU)
    .ascii "ok:cpu\n"
#elif defined(CASE_POLL)
    .ascii "ok:poll\n"
#elif defined(CASE_SLEEP)
    .ascii "ok:sleep\n"
#elif defined(CASE_PIPE)
    .ascii "ok:pipe\n"
#endif
marker_end:
.section .data
.balign 8
request: .quad 0, 5000000
remaining: .quad 0, 0
pipefds: .long 0, 0
payload: .byte 0x5a
received: .byte 0
.section .note.GNU-stack,"",@progbits
