/* MH-2: normal static ELF, one worker, 65536-byte cache, hot threshold 16.
 * Negative slots cover all 32 four-way buckets on a single aligned code page.
 * The register-only credit loop is the only positive before negative pressure;
 * revisiting it replenishes compiler credit without creating new negatives.
 * The later memory loop costs more than that positive plus <399 spare bytes.
 * Require NegativeBudgetEvictions>0 and NativeMemoryInstructions>1000. */
.global _start
.text
_start:
    /* Commit the legal private data page before the later native load. */
    mov datum(%rip), %eax
    cmp $1, %eax
    jne failed
    lea credit_loop(%rip), %r15
    mov $8192, %ecx
    xor %ebx, %ebx
    call *%r15
    cmp $8192, %ebx
    jne failed

    lea negative_slots(%rip), %r12
    mov $128, %r13d
    mov $8192, %r14d
    mov $-1, %eax
negative_call:
    call *%r12                     /* Unsupported indirect CALL stays ordinary. */
negative_return:
    sar $1, %eax                   /* Keep hot caller traces negative too. */
    add $25, %r12
    dec %r13d
    jne negative_call
negative_wrap:
    sar $1, %eax                   /* No profitable reset trace at loop exit. */
    lea negative_slots(%rip), %r12
    mov $128, %r13d
    dec %r14d
    jne negative_call
    cmp $-1, %eax
    jne failed

    /* The resident register loop survives negative-only publication. Its
     * native entries refill the eight compile credits before the new loop. */
    mov $8192, %ecx
    xor %ebx, %ebx
    call *%r15
    cmp $8192, %ebx
    jne failed
    mov $262144, %ecx
    xor %edx, %edx
hot_positive:
    mov datum(%rip), %eax
    add %eax, %edx
    sub $1, %ecx
    jne hot_positive
    cmp $262144, %edx
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

/* Three instructions with an internal backedge qualify at normal threshold.
 * RET is outside the compiled loop and is visited only twice. */
.balign 32
credit_loop:
    add $1, %ebx
    sub $1, %ecx
    jne credit_loop
    ret

/* On this page hash(IP)&31 is (25*n XOR page constants)&31. Since 25 is
 * odd, n=0..127 visits every bucket exactly four times. All 15-byte negative
 * fingerprints stay on the page. Padding is never reached after RET.
 * 8192 sweeps tolerate hot admission, compile-credit throttling and bounded
 * worker negative delays; the entire witness executes fewer than 8.5M guest
 * instructions. No new negative target is visited after these sweeps. */
.balign 4096
negative_slots:
    .rept 128
    sar $1, %eax
    ret
    .fill 22, 1, 0x90
    .endr
negative_slots_end:
    .if negative_slots_end-negative_slots != 128*25
    .error "MH-2 negative slots must have a 25-byte stride"
    .endif
    .if negative_slots_end-negative_slots > 4096-15
    .error "MH-2 negative fingerprints must remain on one code page"
    .endif
.data
.balign 4096
datum:
    .long 1
message:
    .ascii "negative-pressure: ok\n"
message_end:
.section .note.GNU-stack,"",@progbits
