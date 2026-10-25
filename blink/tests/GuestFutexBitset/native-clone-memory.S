/* Ordinary Linux clone + shared-memory publication, without futex syscalls.
 * The child first-touches anonymous BSS pages while the parent polls ready.
 * This independently exercises the pinned native emulator's NOLINEAR page
 * table growth. It does not test the authored futex implementation. */
.global _start
.text
_start:
    movl $0,ready(%rip)
    lea stack_end(%rip),%rsi
    lea tid(%rip),%rdx
    mov %rdx,%r10
    xor %r8d,%r8d
    mov $0x310f00,%edi
    mov $56,%eax
    syscall
    test %rax,%rax
    js fail
    jz child
poll:
    cmpl $1,ready(%rip)
    je success
    mov $24,%eax
    syscall
    jmp poll
child:
    call touch
    movl $1,ready(%rip)
    xor %edi,%edi
    mov $60,%eax
    syscall
    jmp fail
touch:
    lea pages(%rip),%rdi
    mov $1024,%ecx
nextpage:
    movl $42,(%rdi)
    add $4096,%rdi
    loop nextpage
    ret
success:
    xor %edi,%edi
    jmp exit
fail:
    mov $99,%edi
exit:
    mov $231,%eax
    syscall
.data
    .quad 0
.bss
.balign 4096
stack: .skip 65536
stack_end:
ready: .long 0
tid: .long 0
.balign 4096
pages: .skip 4194304
.section .note.GNU-stack,"",@progbits
