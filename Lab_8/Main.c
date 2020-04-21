#include <mpi.h>
#include <stdio.h>
#include <math.h>
#include <assert.h>
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

void multiply_sub_matrix(
    float const * a,
    float const * b,
    float * result,
    size_t size,
    size_t current_size,
    size_t a_row,
    size_t a_col,
    size_t b_row,
    size_t b_col
)
{
    for (size_t i = 0; i < current_size; ++i)
    {
        for (size_t j = 0; j < current_size; ++j)
        {
            float temp = 0.f;
            for (size_t k = 0; k < current_size; ++k)
            {
                temp += a[(i + a_row * current_size) * size + (k + a_col * current_size)] * b[(k + b_row * current_size) * size + (j + b_col * current_size)];
            }
            result[i * current_size + j] += temp;
        }
    }
}

void set_to_result(
    float const * matrix,
    float * c,
    size_t size,
    size_t current_size,
    size_t row,
    size_t col
)
{
    for (size_t i = 0; i < current_size; ++i)
    {
        for (size_t j = 0; j < current_size; ++j)
        {
            c[(i + row * current_size) * size + (j + col * current_size)] = matrix[i * current_size + j];
        }
    }
}


void foxs_method(float const * a, float const * b, float * c, size_t size, size_t my_rank, size_t world_size)
{
    assert(world_size == 4);

    MPI_Status status;

    size_t const dimension = 2;
    size_t const current_size = size / dimension;

    size_t const current_row = my_rank / dimension;
    size_t const current_col = my_rank % dimension;

    size_t const current_row_begin = current_row * current_size;
    size_t const current_row_end = current_row_begin + current_size;
    size_t const current_col_begin = current_col * current_size;
    size_t const current_col_end = current_col_begin + current_size;

    float * matrix = generate_matrix(current_size, TRUE);

    size_t iterations = dimension;
    size_t a_row = current_row;
    size_t a_col = current_col;
    size_t b_row = current_row;
    size_t b_col = current_col;

    for (size_t l = 0; l < iterations; ++l)
    {
        a_row = current_row;
        a_col = current_col;

        a_col = (a_row + l) % iterations;

        multiply_sub_matrix(a, b, matrix, size, current_size, a_row, a_col, b_row, b_col);

        b_row = (b_row + 1) % dimension;
    }

    int const tag = 1;

    if (my_rank == 0)
    {
        set_to_result(matrix, c, size, current_size, 0, 0);

        for (int i = 1; i < (int)world_size; ++i)
        {
            int row;
            int col;

            MPI_Recv(&row, 1, MPI_INT, i, tag, MPI_COMM_WORLD, &status);
            MPI_Recv(&col, 1, MPI_INT, i, tag, MPI_COMM_WORLD, &status);
            MPI_Recv(&matrix[0], (int)(current_size * current_size), MPI_FLOAT, i, tag, MPI_COMM_WORLD, &status);

            set_to_result(matrix, c, size, current_size, row, col);
        }
    }

    if (my_rank != 0)
    {
        int row = (int)current_row;
        int col = (int)current_col;
        MPI_Send(&row, 1, MPI_INT, 0, tag, MPI_COMM_WORLD);
        MPI_Send(&col, 1, MPI_INT, 0, tag, MPI_COMM_WORLD);
        MPI_Send(&matrix[0], (int)(current_size * current_size), MPI_FLOAT, 0, tag, MPI_COMM_WORLD);
    }

    destroy_matrix(matrix, size);
}

double foxs_method_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return calculate_multiply_time(size, my_rank, world_size, foxs_method);
}

void cannon_method(float const * a, float const * b, float * c, size_t size, size_t my_rank, size_t world_size)
{

}

double cannon_method_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return calculate_multiply_time(size, my_rank, world_size, cannon_method);
}


int main()
{
    size_t size = 4;
    char const * name = FOXS_METHOD_ALGO_NAME;
    size_t id = FOXS_METHOD_ALGO_ID;

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