using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DemoGlobalSum
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Preparación de datos (Arreglo grande)
            int tamaño = 900_000_000;
            int[] numeros = Enumerable.Range(1, tamaño).ToArray();
            Console.WriteLine($"--- Suma Global de {tamaño} elementos ---\n");

            // 2. EJECUCIÓN SECUENCIAL
            Stopwatch sw = Stopwatch.StartNew();
            long sumaSecuencial = 0;
            for (int i = 0; i < tamaño; i++) sumaSecuencial += numeros[i];
            sw.Stop();
            long tiempoSecuencial = sw.ElapsedMilliseconds;
            Console.WriteLine($"Secuencial: {sumaSecuencial} | Tiempo: {sw.ElapsedMilliseconds}ms");

            // 3. EJECUCIÓN PARALELA CON TASKS
            sw.Restart();
            int numProcesadores = Environment.ProcessorCount;
            int tamañoLote = tamaño / numProcesadores;
            var tareas = new List<Task<long>>();

            Console.WriteLine($"\nUsando {numProcesadores} núcleos para la suma paralela...");

            for (int i = 0; i < numProcesadores; i++)
            {
                int inicio = i * tamañoLote;
                // El último lote toma el resto si la división no es exacta
                int fin = (i == numProcesadores - 1) ? tamaño : inicio + tamañoLote;

                // Lanzamos una Task<TResult> para cada núcleo, que calcula la suma parcial de su segmento
                tareas.Add(Task.Run(() => {
                    long sumaParcial = 0;
                    for (int j = inicio; j < fin; j++) sumaParcial += numeros[j];
                    return sumaParcial;
                }));
            }

            // Esperamos los resultados y sumamos los parciales 
            long[] resultadosParciales = await Task.WhenAll(tareas);
            long sumaParalela = resultadosParciales.Sum();
            sw.Stop();

            Console.WriteLine($"Paralela:   {sumaParalela} | Tiempo: {sw.ElapsedMilliseconds}ms");

            float mejora = tiempoSecuencial / sw.ElapsedMilliseconds;
            Console.WriteLine($"\nMejora: {mejora:F2}x más rápido que la versión secuencial.");
        }
    }
}