//Author: Mao Weiqing Email: dustonlyperth@gmail.com Date: 2013.01 Nation: Perth, Australia
/*
1. when gpu is not enabled, the app will run via cpu
2. change the variable COUNT to 128 to calculate the 128x128x128 number of prime number. 256, 512 etc. 64 by default
3.It will write all prime numbers from 2 to 64x64x64 to file C:\primenumber\PrimeNumberGPU.txt
4. be aware that when the GPU is computing, the screen is freezed and can't be refreshed
5.after two minutes freezing, Windows will recover the driver be default, the timeing can be changed via Regedit.exe
*/
using System;
using System.Diagnostics;
using System.IO;
using Cloo;

namespace Hello3D
{
    class Program
    {
        static void Main(string[] args)
        {
            #region
            const string programName = "Prime Number";

            Stopwatch stopWatch = new Stopwatch();

            string clProgramSource = KernelProgram();

            Console.WriteLine("Environment OS:");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine(Environment.OSVersion);
            #endregion
            if (ComputePlatform.Platforms.Count == 0)
            {
                Console.WriteLine("No OpenCL Platforms are availble!");
            }
            else
            {
                #region 1
                // step 1 choose the first available platform
                ComputePlatform platform = ComputePlatform.Platforms[0];

                // output the basic info
                BasicInfo(platform);

                Console.WriteLine("Program: " + programName);
                Console.WriteLine("-----------------------------------------");
                #endregion
                //Cpu 10 seconds Gpu 28 seconds
                int count = 64;

                int[] output_Z = new int[count * count * count];

                int[] input_X = new int[count * count * count];

                for (int x = 0; x < count * count * count; x++)
                {
                    input_X[x] = x;
                }
                #region 2
                // step 2 create context for that platform and all devices
                ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
                ComputeContext context = new ComputeContext(platform.Devices, properties, null, IntPtr.Zero);

                // step 3 create and build program
                ComputeProgram program = new ComputeProgram(context, clProgramSource);
                program.Build(platform.Devices, null, null, IntPtr.Zero);
                #endregion
                // step 4 create memory objects
                ComputeBuffer<int> a = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, input_X);
                ComputeBuffer<int> z = new ComputeBuffer<int>(context, ComputeMemoryFlags.WriteOnly, output_Z.Length);

                // step 5 create kernel object with same kernel programe name VectorAdd
                ComputeKernel kernel = program.CreateKernel("PrimeNumber");

                // step 6 set kernel arguments
                //kernel.SetMemoryArgument(0, a);
                kernel.SetMemoryArgument(0, a);
                kernel.SetMemoryArgument(1, z);

                ComputeEventList eventList = new ComputeEventList();

                //for (int j = 0; j < context.Devices.Count; j++)
                // query available devices n,...,1,0.  cpu first then gpu
                for (int j = context.Devices.Count-1; j > -1; j--)
                {
                    #region 3
                    stopWatch.Start();

                    // step 7 create command queue on that context on that device
                    ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[j], ComputeCommandQueueFlags.None);

                    // step 8 run the kernel program
                    commands.Execute(kernel, null, new long[] { count, count, count }, null, eventList);
                    //Application.DoEvents();
                    
                    #endregion
                    // step 9 read results
                    commands.ReadFromBuffer(z, ref output_Z, false, eventList);
                    #region 4
                    commands.Finish();

                    string fileName = "C:\\primenumber\\PrimeNumberGPU.txt";
                    StreamWriter file = new StreamWriter(fileName, true);

                    FileInfo info = new FileInfo(fileName);
                    long fs = info.Length;

                    // 1 MegaByte = 1.049e+6 Byte
                    int index = 1;
                    if (fs == 1.049e+6)
                    {
                        fileName = "C:\\primenumber\\PrimeNumberGPU" + index.ToString() + ".txt";
                        file = new System.IO.StreamWriter(fileName, true);
                        index++;
                    }
                    #endregion

                    for (uint xx = 0; xx < count * count * count; xx++)
                    {
                        if (output_Z[xx] != 0 && output_Z[xx] != 1)
                        {
                            Console.WriteLine(output_Z[xx]);
                            file.Write(output_Z[xx]);
                            file.Write("x");
                        }
                    }
                    #region 5
                    file.Close();
                    stopWatch.Stop();

                    ComputeCommandProfilingInfo start = ComputeCommandProfilingInfo.Started;
                    ComputeCommandProfilingInfo end = ComputeCommandProfilingInfo.Ended;
                    double time = 10e-9 * (end - start);
                    //Console.WriteLine("Nanosecond: " + time);


                    TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                    Console.WriteLine(context.Devices[j].Name.Trim() + " Elapsed Time " + elapsedTime);

                    Console.WriteLine("-----------------------------------------");
                    #endregion
                }
                Console.ReadLine();
            }
        }

        private static string KernelProgram()
        {
            // local variable inside of a kernel can't be initilized at first time
            // eg. _local float x; x = 4.0;
            // transfer a data from a memory object, to a kernel parameter, it has to be global.
            // kernel program name: VectorAdd
            string clProgramSource = @"
           int isPrimeNumber(int num)
           {
            for (int i = 2; i < num; i++)
            {
                if (num % i == 0)
                    return 0;
            }
            return num;
           }
           kernel void PrimeNumber(global read_only int * input_X, global write_only int * output_Z)
           {
                int i = get_global_id(0);
                int j = get_global_id(1);
                int k = get_global_id(2);
                int p = get_global_size(0);

                output_Z[i+p*j+p*p*k] = isPrimeNumber(input_X[i+p*j+p*p*k]);
           }
           ";
           return clProgramSource;
        }

        /*
                long i = get_global_id(0);
                long j = get_global_id(1);

                long p = get_global_size(0);
                long r = get_global_size(1);
                
                output_Z[i+p*j] = isPrimeNumber(input_Z[i+p*j]);
                
                i+2*j+2*2*k
                j i k   value
                
                0 0 0  0
         *      0 1 0  1
         *      1 0 0  2
         *      1 1 0  3
         *      0 0 1  4
         *      0 1 1  5
         *      1 0 1  6
         *      1 1 1  7
        */

        private static void BasicInfo(ComputePlatform platform)
        {

            Console.WriteLine("");
            Console.WriteLine("Platform Information:");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Name: " + platform.Name);
            Console.WriteLine("Profile: " + platform.Profile);
            Console.WriteLine("Vendor: " + platform.Vendor);
            Console.WriteLine("Version: " + platform.Version);
            Console.WriteLine("Extensions:");
            foreach (string extension in platform.Extensions)
                Console.WriteLine(" + " + extension);

            Console.WriteLine("Devices:");

            if (platform.Devices.Count == 0)
            {
                Console.WriteLine("No OpenCL devices are availble!");
            }
            else
            {
                for (int i = 0; i < platform.Devices.Count; i++)
                    Console.WriteLine(" + " + platform.Devices[i].Name.Trim());
            }

            Console.WriteLine("");
        }
    }
}
