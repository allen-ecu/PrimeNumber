PrimeNumber
===========

Calculate NxNxN number of prime numbers (OpenCL AMDAPP)

Task:
Calculate NxNxN number of prime numbers

Environment:
OpenCL(Cloo 0.9)  AMD-APP1084.4 AMD CATALYST13.1 
Microsoft Visual Studio Pro 2010 x64 with SP1
GPU: AMD 6750M CPU: Intel Core i5-2410M 2.3GHz RAM: 4GB
OS: Windows7 PRO SP1 x64

Objective:
Use GPU to computing in concurrent efficiently

Steps:

1. when gpu is not enabled, the app will run via cpu
2. change the variable COUNT to 128 to calculate the 128x128x128 number of prime number. 256, 512 etc. 64 by default
3.It will write all prime numbers from 2 to 64x64x64 to file C:\primenumber\PrimeNumberGPU.txt
4. be aware that when the GPU is computing, the screen is freezed and can't be refreshed
5.after two minutes freezing, Windows will recover the driver be default, the timeing can be changed via Regedit.exe

//Author: Mao Weiqing Email: dustonlyperth@gmail.com Date: 2013.01 Nation: Perth, Australia
