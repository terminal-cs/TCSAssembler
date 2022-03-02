[org 0x7c00]
jmp KernelEntry
; Main inside of $Source
; first param: System.Int32
Source.Main:
Source.Main.another_num:
	.size dd 0
; another_num type: System.Int32
Source.Kernel.num: dq 0
times 510-($-$$) db 0
dw 0xAA55
