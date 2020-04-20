#include <mpi.h>
#include <stdio.h>
#include <math.h>
#include <windows.h>


#define CONSISTENT_ALGO_NAME "consistent"
#define TAPE_CIRCUIT_ALGO_NAME "tape circuit"
#define FOXS_METHOD_ALGO_NAME "fox's method"
#define CANNON_METHOD_ALGO_NAME "cannon method"

#define CONSISTENT_ALGO_ID 0 
#define TAPE_CIRCUIT_ALGO_ID 1
#define FOXS_METHOD_ALGO_ID 2
#define CANNON_METHOD_ALGO_ID 3


float ** generate_matrix(int size, BOOL is_zero_fill)
{
    float ** a = (float **)malloc(size * sizeof(float *));
    for (int i = 0; i < size; ++i)
    {
        a[i] = (float *)malloc(size * sizeof(float));
        for (int j = 0; j < size; ++j)
        {
            a[i][j] = 0.f;
            if (is_zero_fill != TRUE)
            {
                a[i][j] = (float)(rand() % 1000) - 500.f;
            }
        }
    }

    return a;
}

void destroy_matrix(float ** a, int size)
{
    for (int i = 0; i < size; ++i)
    {
        free(a[i]);
    }
    free(a);
}


void consistent(float ** a, float ** b, float ** c, int size)
{
    for (int i = 0; i < size; ++i)
    {
        for (int j = 0; j < size; ++j)
        {
            for (int k = 0; k < size; ++k)
            {
                c[i][j] += a[i][k] * b[k][j];
            }
        }
    }
}


double consistent_multiply_time(int size, int my_rank, int world_size)
{
    if (my_rank != 0)
    {
        return HUGE_VAL;
    }

    LARGE_INTEGER frequency;
    LARGE_INTEGER t1;
    LARGE_INTEGER t2;

    QueryPerformanceFrequency(&frequency);

    float ** a = generate_matrix(size, FALSE);
    float ** b = generate_matrix(size, FALSE);
    float ** c = generate_matrix(size, TRUE);

    QueryPerformanceCounter(&t1);

    consistent(a, b, c, size);

    QueryPerformanceCounter(&t2);
   
    destroy_matrix(a, size);
    destroy_matrix(b, size);
    destroy_matrix(c, size);

    return 1000.f * ((double)(t2.QuadPart - t1.QuadPart) / (double)frequency.QuadPart);
}


int main()
{
    int size = 100;
    char const * name = CONSISTENT_ALGO_NAME;
    int id = CONSISTENT_ALGO_ID;

    int my_rank;
    int world_size;

    MPI_Init(NULL, NULL);

    MPI_Comm_size(MPI_COMM_WORLD, &world_size);
    MPI_Comm_rank(MPI_COMM_WORLD, &my_rank);

    printf("%d, %d\n", world_size, my_rank);

    double delta = HUGE_VAL;

    if (id == CONSISTENT_ALGO_ID)
    {
        delta = consistent_multiply_time(size, my_rank, world_size);
    }

    if (my_rank == 0)
    {
        printf("Algo %s, time %f ms\n", name, delta);
    }

    MPI_Finalize();
    return 0;
}