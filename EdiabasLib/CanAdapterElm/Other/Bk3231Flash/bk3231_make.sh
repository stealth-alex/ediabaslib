#!/bin/sh
# Use in mingw shell
# KEIL 5 ARM MDK must be installed at: /c/Programs/Keil_v5
# flash_write_area D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/Other/Bk3231Flash/write_flash.bin 0x3E000

export PATH=$PATH:/c/Programs/Keil_v5/ARM/ARMCC/bin/
make -k -B
