/* Ordinary memory syscall contract. Compile with PROFILE for explicit limits. */
.global _start
.text
_start:
    mov $9,%eax
    xor %edi,%edi
    mov $12288,%esi
    mov $3,%edx
    mov $34,%r10d
    mov $-1,%r8
    xor %r9d,%r9d
    syscall
    test %rax,%rax
    js fail
    mov %rax,%r12
    movb $42,(%r12)
    movb $43,4096(%r12)
    movb $44,8192(%r12)
    lea 4096(%r12),%rdi
    mov $1,%esi                 /* round length up, discard exactly one page */
    call discard
    lea 4096(%r12),%rdi
    mov $1,%esi
    call discard                /* a reservation must stay lazy and zeroed */
    cmpb $0,4096(%r12)
    jne stale
    cmpb $42,(%r12)
    jne fail
    cmpb $44,8192(%r12)
    jne fail
    movb $51,4096(%r12)
    lea 4096(%r12),%rdi
    mov $4096,%esi
    xor %edx,%edx
    call protect
    lea 4096(%r12),%rdi
    mov $4096,%esi
    call discard                /* no guest read/write permission required */
    lea 4096(%r12),%rdi
    mov $4096,%esi
    mov $1,%edx
    call protect
    cmpb $0,4096(%r12)
    jne fail
    lea 4096(%r12),%rdi
    mov $4096,%esi
    call discard                /* committed read-only backing is supported */
    cmpb $0,4096(%r12)
    jne fail
    mov %r12,%rdi
    xor %esi,%esi
    call discard                /* zero length succeeds */
    mov %r12,%rdi
    mov $12288,%esi
    xor %ebp,%ebp
hints:
    mov %ebp,%edx
    mov $28,%eax
    syscall
    test %rax,%rax
    jne fail
    inc %ebp
    cmp $4,%ebp
    jne hints
    lea 1(%r12),%rdi
    mov $1,%esi
    mov $4,%edx
    mov $28,%eax
    syscall
    cmp $-22,%rax
    jne fail
    mov %r12,%rdi
    mov $-1,%rsi
    mov $28,%eax
    syscall
    cmp $-22,%rax
    jne fail
    mov %r12,%rdi
    mov $4096,%esi
    mov $9999,%edx
    mov $28,%eax
    syscall
    cmp $-22,%rax
    jne fail
    cmpb $42,(%r12)
    jne fail
    mov $0x1000000000000,%rdi    /* must not canonicalize into page zero */
    mov $4096,%esi
    mov $4,%edx
    mov $28,%eax
    syscall
    cmp $-12,%rax
    jne fail
    lea 4096(%r12),%rdi
    mov $4096,%esi
    mov $11,%eax
    syscall
    test %rax,%rax
    jne fail
    mov %r12,%rdi
    mov $12288,%esi
    mov $4,%edx
    mov $28,%eax
    syscall
    cmp $-12,%rax
    jne fail
    cmpb $0,(%r12)              /* Linux still discards mapped sides of hole */
    jne fail
    cmpb $0,8192(%r12)
    jne fail
    mov %r12,%rdi
    mov $12288,%esi
    mov $11,%eax
    syscall
    test %rax,%rax
    jne fail
    lea file(%rip),%rdi
    xor %esi,%esi
    xor %edx,%edx
    mov $2,%eax
    syscall
    test %rax,%rax
    js fail
    mov %rax,%r13
    mov %rax,%r8
    mov $9,%eax
    xor %edi,%edi
    mov $4096,%esi
    mov $3,%edx
    mov $2,%r10d
    xor %r9d,%r9d
    syscall
    test %rax,%rax
    js fail
    mov %rax,%r12
    movb $77,(%r12)
    mov %r12,%rdi
    mov $4096,%esi
    mov $4,%edx
    mov $28,%eax
    syscall
#ifdef PROFILE
    cmp $-95,%rax              /* file reload is explicitly unsupported */
    jne fail
    cmpb $77,(%r12)
    jne fail
    mov %r12,%rdi
    mov $4096,%esi
    mov $16,%edx               /* DONTDUMP may not silently succeed */
    mov $28,%eax
    syscall
    cmp $-22,%rax
    jne fail
#else
    test %rax,%rax
    jne fail
    cmpb $99,(%r12)
    jne fail
#endif
    mov %r12,%rdi
    mov $4096,%esi
    mov $11,%eax
    syscall
    test %rax,%rax
    jne fail
    mov %r13,%rdi
    mov $3,%eax
    syscall
    test %rax,%rax
    jne fail
    xor %edi,%edi
    jmp exit
discard:
    mov $4,%edx
    mov $28,%eax
    syscall
    test %rax,%rax
    jne fail
    ret
protect:
    mov $10,%eax
    syscall
    test %rax,%rax
    jne fail
    ret
stale:
    mov $42,%edi
    jmp exit
fail:
    mov $99,%edi
exit:
    mov $231,%eax
    syscall
.section .data
file: .asciz "advice-data"
.section .note.GNU-stack,"",@progbits
