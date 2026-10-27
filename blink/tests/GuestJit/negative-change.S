/* MH-1: ordinary private code, RW fill -> RX calls -> RW suffix patch -> RX.
 * Run with one worker and HotBlockThreshold=16. The first three instructions
 * occupy exactly 15 bytes; SAR makes that prefix unprofitable before mutation.
 * No child is created, so the private-code admission path remains eligible. */
.global _start
.text
_start:
    cld
    mov $9, %eax                    /* mmap(NULL, 4096, RW, PRIVATE|ANON, -1, 0) */
    xor %edi, %edi
    mov $4096, %esi
    mov $3, %edx
    mov $34, %r10d
    mov $-1, %r8
    xor %r9d, %r9d
    syscall
    cmp $-4095, %rax
    jae failed
    mov %rax, %r12
    mov %r12, %rdi
    lea initial_code(%rip), %rsi
    mov $initial_end-initial_code, %ecx
    rep movsb
    mov $10, %eax                   /* mprotect(page, 4096, RX) */
    mov %r12, %rdi
    mov $4096, %esi
    mov $5, %edx
    syscall
    test %rax, %rax
    jne failed

    /* The indirect CALL interprets, then observes the helper's entry IP.
     * Repetition crosses normal hot admission and creates a real negative. */
    mov $1024, %r13d
before_change:
    call *%r12
    cmp $1, %eax
    jne failed
    dec %r13d
    jne before_change

    mov $10, %eax                   /* mprotect(page, 4096, RW) */
    mov %r12, %rdi
    mov $4096, %esi
    mov $3, %edx
    syscall
    test %rax, %rax
    jne failed
    lea 15(%r12), %rdi              /* Bytes [0,15) are never rewritten. */
    lea supported_suffix(%rip), %rsi
    mov $supported_end-supported_suffix, %ecx
    rep movsb
    mov $10, %eax                   /* mprotect(page, 4096, RX), same VA */
    mov %r12, %rdi
    mov $4096, %esi
    mov $5, %edx
    syscall
    test %rax, %rax
    jne failed

    /* Each call leaves the helper's 64-byte interpreter window at RET.
     * 65536 calls outlast even the 16384-observation worker negative bound.
     * Exit correctness alone is insufficient: require NegativePromotions>0. */
    mov $65536, %r13d
after_change:
    call *%r12
    cmp $60, %eax
    jne failed
    dec %r13d
    jne after_change
    mov $11, %eax                   /* munmap(page, 4096) */
    mov %r12, %rdi
    mov $4096, %esi
    syscall
    test %rax, %rax
    jne failed
    mov $1, %eax
    mov $1, %edi
    lea message(%rip), %rsi
    mov $message_end-message, %edx
    syscall
    cmp $message_end-message, %rax
    jne failed
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall

/* Templates are data, so copying them does not revoke the caller's code. */
.data
initial_code:
    mov $2, %eax
    mov $7, %ecx
    mov $11, %edx
initial_suffix:
    sar $1, %eax
    ret
    .fill 32-(.-initial_code), 1, 0x90
initial_end:
    .if initial_suffix-initial_code != 15
    .error "MH-1 fingerprint prefix must occupy exactly 15 bytes"
    .endif
supported_suffix:
    add $40, %eax
    add %ecx, %eax
    add %edx, %eax
    ret
supported_end:
    .if supported_end-supported_suffix > 32-15
    .error "MH-1 replacement must fit the initialized code buffer"
    .endif
message:
    .ascii "negative-change: ok\n"
message_end:
.section .note.GNU-stack,"",@progbits
