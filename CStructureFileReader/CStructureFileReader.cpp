// CStructureFileReader.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <stdint.h>
#include <stdio.h>

// DataRec 16 Bytes Total
typedef struct {
	time_t   TimeStamp; // +4 bytes = 4 bytes
	uint32_t IntTemp	: 10;
	uint32_t MinIntTemp : 10;
	uint32_t MaxIntTemp : 10;
	uint32_t Res1		: 2;  // +4 bytes = 8 bytes
	uint32_t IntRH	: 10;
	uint32_t MinIntRH	: 10;
	uint32_t MaxIntRH : 10;
	uint32_t Res2		: 2;  // +4 bytes = 12 bytes
	uint32_t ExtTemp	: 10;
	uint32_t ExtRH	: 10;
	uint32_t Res3		: 12; // +4 bytes = 16 bytes
} DataRec;



int main()
{
	FILE *f;
	DataRec d;

	printf("File Contents");
	
	f = fopen("C:\\DATALOG.100", "r");

	fread(&d, sizeof(d), 1, f);
		
    return 0;
}

