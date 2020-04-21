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


float * generate_matrix(size_t size, BOOL is_zero_fill)
{
    float * a = (float *)malloc(size * size * sizeof(float));
    for (size_t i = 0; i < size; ++i)
    {
        for (size_t j = 0; j < size; ++j)
        {
            a[i * size + j] = 0.f;
            if (is_zero_fill != TRUE)
            {
                a[i * size + j] = ((float)(rand() % 1000) - 500.f) * 0.01f;
            }
        }
    }

    return a;
}

void destroy_matrix(float * a, size_t size)
{
    free(a);
}


void consistent(float const * a, float const * b, float * c, size_t size)
{
    for (size_t i = 0; i < size; ++i)
    {
        for (size_t j = 0; j < size; ++j)
        {
            float temp = 0.f;
            for (size_t k = 0; k < size; ++k)
            {
                temp += a[i * size + k] * b[k * size + j];
            }
            c[i * size + j] = temp;
        }
    }
}

double consistent_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    if (my_rank != 0)
    {
        return HUGE_VAL;
    }

    LARGE_INTEGER frequency;
    LARGE_INTEGER t1;
    LARGE_INTEGER t2;

    QueryPerformanceFrequency(&frequency);

    float * a = generate_matrix(size, FALSE);
    float * b = generate_matrix(size, FALSE);
    float * c = generate_matrix(size, TRUE);

    QueryPerformanceCounter(&t1);

    consistent(a, b, c, size);

    QueryPerformanceCounter(&t2);

    destroy_matrix(a, size);
    destroy_matrix(b, size);
    destroy_matrix(c, size);

    return (double)(t2.QuadPart - t1.QuadPart) / (double)frequency.QuadPart;
}

double calculate_multiply_time(
    size_t size,
    size_t my_rank,
    size_t world_size,
    void (*multiplier)(float const *, float const *, float *, size_t, size_t, size_t)
)
{
    LARGE_INTEGER frequency;
    LARGE_INTEGER t1;
    LARGE_INTEGER t2;

    QueryPerformanceFrequency(&frequency);

    float * a = generate_matrix(size, FALSE);
    float * b = generate_matrix(size, FALSE);
    float * c = generate_matrix(size, TRUE);
    float * correct = generate_matrix(size, TRUE);

    consistent(a, b, correct, size);

    QueryPerformanceCounter(&t1);

    multiplier(a, b, c, size, my_rank, world_size);

    QueryPerformanceCounter(&t2);

    if (my_rank == 0)
    {
        printf("******\n");
        for (size_t i = 0; i < size; ++i)
        {
            for (size_t j = 0; j < size; ++j)
            {
                printf("%f ", correct[i * size + j]);
            }
            printf("\n");
        }

        printf("******\n");

        for (size_t i = 0; i < size; ++i)
        {
            for (size_t j = 0; j < size; ++j)
            {
                printf("%f ", c[i * size + j]);
            }
            printf("\n");
        }
        printf("******\n");
    }

    destroy_matrix(a, size);
    destroy_matrix(b, size);
    destroy_matrix(c, size);
    destroy_matrix(correct, size);

    return (double)(t2.QuadPart - t1.QuadPart) / (double)frequency.QuadPart;
}


void tape_circuit(float const * a, float const * b, float * c, size_t size, size_t my_rank, size_t world_size)
{
    size_t const current_size = (size / world_size);
    size_t const begin = current_size * my_rank;
    size_t const end = begin + current_size;

    MPI_Status status;

    for (size_t i = begin; i < end; ++i)
    {
        for (size_t j = 0; j < size; ++j)
        {
            float temp = 0.f;
            for (size_t k = 0; k < size; ++k)
            {
                temp += a[i * size + k] * b[k * size + j];
            }
            c[i * size + j] = temp;
        }
    }

    int const tag = 1;

    if (my_rank == 0)
    {
        for (int i = 1; i < (int)world_size; ++i)
        {
            int offset;

            MPI_Recv(&offset, 1, MPI_INT, i, tag, MPI_COMM_WORLD, &status);
            MPI_Recv(&c[offset], (int)(current_size * size), MPI_FLOAT, i, tag, MPI_COMM_WORLD, &status);
        }
    }

    if (my_rank != 0)
    {
        int const offset = (int)(begin * size);
        MPI_Send(&offset, 1, MPI_INT, 0, tag, MPI_COMM_WORLD);
        MPI_Send(&c[offset], (int)(current_size * size), MPI_FLOAT, 0, tag, MPI_COMM_WORLD);
    }
}

double tape_circuit_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return calculate_multiply_time(size, my_rank, world_size, tape_circuit);
}

double foxs_method_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return HUGE_VAL;
}

double cannon_method_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return HUGE_VAL;
}


int main()
{
    size_t size = 4;
    char const * name = TAPE_CIRCUIT_ALGO_NAME;
    size_t id = TAPE_CIRCUIT_ALGO_ID;

    int my_rank;
    int world_size;

    MPI_Init(NULL, NULL);

    MPI_Comm_size(MPI_COMM_WORLD, &world_size);
    MPI_Comm_rank(MPI_COMM_WORLD, &my_rank);

    printf("%d, %d\n", world_size, my_rank);

    double delta = HUGE_VAL;

    if (id == CONSISTENT_ALGO_ID)
    {
        delta = consistent_multiply_time(size, (size_t)my_rank, (size_t)world_size);
    }
    if (id == TAPE_CIRCUIT_ALGO_ID)
    {
        delta = tape_circuit_multiply_time(size, (size_t)my_rank, (size_t)world_size);
    }
    if (id == FOXS_METHOD_ALGO_ID)
    {
        delta = foxs_method_multiply_time(size, (size_t)my_rank, (size_t)world_size);
    }
    if (id == CANNON_METHOD_ALGO_ID)
    {
        delta = cannon_method_multiply_time(size, (size_t)my_rank, (size_t)world_size);
    }

    if (my_rank == 0)
    {
        printf("Algo %s, time %f s\n", name, delta);
    }

    MPI_Finalize();
    return 0;
}