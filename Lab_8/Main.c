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

#define MAIN_PROCESS 0


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

void destroy_matrix(float * a)
{
    free(a);
}


void consistent(float const * a, float const * b, float * c, size_t size)
{
    for (size_t i = 0; i < size; ++i)
    {
        for (size_t j = 0; j < size; ++j)
        {
            float temp = c[i * size + j];
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

    destroy_matrix(a);
    destroy_matrix(b);
    destroy_matrix(c);

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

    float * a = NULL;
    float * b = NULL;
    float * c = NULL;

    if (my_rank == MAIN_PROCESS)
    {
        a = generate_matrix(size, FALSE);
        b = generate_matrix(size, FALSE);
        c = generate_matrix(size, TRUE);
    }

    QueryPerformanceCounter(&t1);

    multiplier(a, b, c, size, my_rank, world_size);

    QueryPerformanceCounter(&t2);

    if (my_rank == MAIN_PROCESS)
    {
        destroy_matrix(a);
        destroy_matrix(b);
        destroy_matrix(c);
    }

    return (double)(t2.QuadPart - t1.QuadPart) / (double)frequency.QuadPart;
}


void tape_circuit(float const * a, float const * b, float * c, size_t size, size_t my_rank, size_t world_size)
{
    size_t const current_size = (size / world_size);
    size_t const begin = current_size * my_rank;
    size_t const end = begin + current_size;

    MPI_Status status;
    int const tag = 1;

    float * a_matrix = NULL;
    float * b_matrix = NULL;
    float * c_matrix = NULL;

    if (my_rank == MAIN_PROCESS)
    {
        for (int i = 1; i < (int)world_size; ++i)
        {
            MPI_Send(&b[0], (int)(size * size), MPI_FLOAT, i, tag, MPI_COMM_WORLD);
            MPI_Send(&a[size * current_size * (size_t)i], (int)(size * current_size), MPI_FLOAT, i, tag, MPI_COMM_WORLD);
        }
    }

    if (my_rank != MAIN_PROCESS)
    {
        a_matrix = (float *)malloc(size * current_size * sizeof(float));
        b_matrix = (float *)malloc(size * size * sizeof(float));
        c_matrix = (float *)malloc(size * current_size * sizeof(float));

        MPI_Recv(&b_matrix[0], (int)(size * size), MPI_FLOAT, MAIN_PROCESS, tag, MPI_COMM_WORLD, &status);
        MPI_Recv(&a_matrix[0], (int)(size * current_size), MPI_FLOAT, MAIN_PROCESS, tag, MPI_COMM_WORLD, &status);
    }

    if (my_rank == MAIN_PROCESS)
    {
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
    }
    if (my_rank != MAIN_PROCESS)
    {
        for (size_t i = 0; i < current_size; ++i)
        {
            for (size_t j = 0; j < size; ++j)
            {
                float temp = 0.f;
                for (size_t k = 0; k < size; ++k)
                {
                    temp += a_matrix[i * size + k] * b_matrix[k * size + j];
                }
                c_matrix[i * size + j] = temp;
            }
        }
    }

    if (my_rank == MAIN_PROCESS)
    {
        for (int i = 1; i < (int)world_size; ++i)
        {
            MPI_Recv(&c[size * current_size * (size_t)i], (int)(current_size * size), MPI_FLOAT, i, tag, MPI_COMM_WORLD, &status);
        }
    }

    if (my_rank != MAIN_PROCESS)
    {
        MPI_Send(&c_matrix[0], (int)(current_size * size), MPI_FLOAT, MAIN_PROCESS, tag, MPI_COMM_WORLD);
    }

    if (my_rank != MAIN_PROCESS)
    {
        destroy_matrix(a_matrix);
        destroy_matrix(b_matrix);
        destroy_matrix(c_matrix);
    }
}

double tape_circuit_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return calculate_multiply_time(size, my_rank, world_size, tape_circuit);
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
    int const tag = 1;

    size_t const dimension = 2;
    size_t const current_size = size / dimension;

    size_t const current_row = my_rank / dimension;
    size_t const current_col = my_rank % dimension;

    size_t const current_row_begin = current_row * current_size;
    size_t const current_row_end = current_row_begin + current_size;
    size_t const current_col_begin = current_col * current_size;
    size_t const current_col_end = current_col_begin + current_size;

    float * native_a_matrix = (float *)malloc(current_size * current_size * sizeof(float));
    float * additional_a_matrix = (float *)malloc(current_size * current_size * sizeof(float));
    float * a_matrix = (float *)malloc(current_size * current_size * sizeof(float));
    float * b_matrix = (float *)malloc(current_size * current_size * sizeof(float));
    float * c_matrix = generate_matrix(current_size, TRUE);

    if (my_rank == MAIN_PROCESS)
    {
        for (size_t j = 0; j < current_size; ++j)
        {
            memcpy(&a_matrix[j * current_size], &a[j * size], current_size * sizeof(float));
        }
        for (size_t j = 0; j < current_size; ++j)
        {
            memcpy(&b_matrix[j * current_size], &b[j * size], current_size * sizeof(float));
        }

        for (size_t i = 1; i < world_size; ++i)
        {
            size_t const row = i / dimension;
            size_t const col = i % dimension;
            for (size_t j = 0; j < current_size; ++j)
            {
                MPI_Send(&a[j * size + row * current_size * size + col * current_size], (int)current_size, MPI_FLOAT, (int)i, tag, MPI_COMM_WORLD);
            }
            for (size_t j = 0; j < current_size; ++j)
            {
                MPI_Send(&b[j * size + row * current_size * size + col * current_size], (int)current_size, MPI_FLOAT, (int)i, tag, MPI_COMM_WORLD);
            }
        }
    }

    if (my_rank != MAIN_PROCESS)
    {
        for (size_t j = 0; j < current_size; ++j)
        {
            MPI_Recv(&a_matrix[j * current_size], (int)current_size, MPI_FLOAT, MAIN_PROCESS, tag, MPI_COMM_WORLD, &status);
        }
        for (size_t j = 0; j < current_size; ++j)
        {
            MPI_Recv(&b_matrix[j * current_size], (int)current_size, MPI_FLOAT, MAIN_PROCESS, tag, MPI_COMM_WORLD, &status);
        }
    }

    memcpy(&native_a_matrix[0], &a_matrix[0], current_size * current_size * sizeof(float));

    size_t const iterations = dimension;
    size_t const a_row = current_row;
    size_t const a_col = current_col;
    size_t const b_row = current_row;
    size_t const b_col = current_col;

    for (size_t l = 0; l < iterations; ++l)
    {
        memcpy(&a_matrix[0], &native_a_matrix[0], current_size * current_size * sizeof(float));

        size_t const j = (a_row + l) % dimension;

        if (current_col == j)
        {
            for (size_t i = 0; i < dimension; ++i)
            {
                if (i != current_col)
                {
                    MPI_Send(&a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, (int)(a_row * dimension + i), tag, MPI_COMM_WORLD);
                }
            }
        }
        if (current_col != j)
        {
            MPI_Recv(&a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, (int)(a_row * dimension + j), tag, MPI_COMM_WORLD, &status);
        }

        consistent(a_matrix, b_matrix, c_matrix, current_size);

        if (l + 1 != iterations)
        {
            size_t const b_next_row = (b_row + 1) % dimension;
            size_t const b_previous_row = (b_row + dimension - 1) % dimension;
            int const send_to = (int)(b_next_row * dimension + current_col);
            int const recv_from = (int)(b_previous_row * dimension + current_col);
            int const current_rank = (int)my_rank;

            if (current_rank < recv_from)
            {
                MPI_Recv(&additional_a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, recv_from, tag, MPI_COMM_WORLD, &status);
                MPI_Send(&b_matrix[0], (int)(current_size * current_size), MPI_FLOAT, send_to, tag, MPI_COMM_WORLD);
                memcpy(&b_matrix[0], &additional_a_matrix[0], current_size * current_size * sizeof(float));
            }
            if (current_rank > recv_from)
            {
                MPI_Send(&b_matrix[0], (int)(current_size * current_size), MPI_FLOAT, send_to, tag, MPI_COMM_WORLD);
                MPI_Recv(&b_matrix[0], (int)(current_size * current_size), MPI_FLOAT, recv_from, tag, MPI_COMM_WORLD, &status);
            }
        }
    }


    if (my_rank == 0)
    {
        set_to_result(c_matrix, c, size, current_size, 0, 0);

        for (int i = 1; i < (int)world_size; ++i)
        {
            int const row = i / (int)dimension;
            int const col = i % (int)dimension;

            MPI_Recv(&c_matrix[0], (int)(current_size * current_size), MPI_FLOAT, i, tag, MPI_COMM_WORLD, &status);

            set_to_result(c_matrix, c, size, current_size, row, col);
        }
    }

    if (my_rank != 0)
    {
        MPI_Send(&c_matrix[0], (int)(current_size * current_size), MPI_FLOAT, 0, tag, MPI_COMM_WORLD);
    }

    destroy_matrix(a_matrix);
    destroy_matrix(b_matrix);
    destroy_matrix(c_matrix);
    destroy_matrix(native_a_matrix);
    destroy_matrix(additional_a_matrix);
}

double foxs_method_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return calculate_multiply_time(size, my_rank, world_size, foxs_method);
}

void cannon_method(float const * a, float const * b, float * c, size_t size, size_t my_rank, size_t world_size)
{
    assert(world_size == 4);

    MPI_Status status;
    int const tag = 1;

    size_t const dimension = 2;
    size_t const current_size = size / dimension;

    size_t const current_row = my_rank / dimension;
    size_t const current_col = my_rank % dimension;

    size_t const current_row_begin = current_row * current_size;
    size_t const current_row_end = current_row_begin + current_size;
    size_t const current_col_begin = current_col * current_size;
    size_t const current_col_end = current_col_begin + current_size;

    float * additional_a_matrix = (float *)malloc(current_size * current_size * sizeof(float));
    float * a_matrix = (float *)malloc(current_size * current_size * sizeof(float));
    float * b_matrix = (float *)malloc(current_size * current_size * sizeof(float));
    float * c_matrix = generate_matrix(current_size, TRUE);

    if (my_rank == MAIN_PROCESS)
    {
        for (size_t j = 0; j < current_size; ++j)
        {
            memcpy(&a_matrix[j * current_size], &a[j * size], current_size * sizeof(float));
        }
        for (size_t j = 0; j < current_size; ++j)
        {
            memcpy(&b_matrix[j * current_size], &b[j * size], current_size * sizeof(float));
        }

        for (size_t i = 1; i < world_size; ++i)
        {
            size_t const row = i / dimension;
            size_t const col = i % dimension;

            size_t const a_row = row;
            size_t const a_col = (col + row) % dimension;
            size_t const b_row = (col + row) % dimension;
            size_t const b_col = col;

            for (size_t j = 0; j < current_size; ++j)
            {
                MPI_Send(&a[j * size + a_row * current_size * size + a_col * current_size], (int)current_size, MPI_FLOAT, (int)i, tag, MPI_COMM_WORLD);
            }
            for (size_t j = 0; j < current_size; ++j)
            {
                MPI_Send(&b[j * size + b_row * current_size * size + b_col * current_size], (int)current_size, MPI_FLOAT, (int)i, tag, MPI_COMM_WORLD);
            }
        }
    }

    if (my_rank != MAIN_PROCESS)
    {
        for (size_t j = 0; j < current_size; ++j)
        {
            MPI_Recv(&a_matrix[j * current_size], (int)current_size, MPI_FLOAT, MAIN_PROCESS, tag, MPI_COMM_WORLD, &status);
        }
        for (size_t j = 0; j < current_size; ++j)
        {
            MPI_Recv(&b_matrix[j * current_size], (int)current_size, MPI_FLOAT, MAIN_PROCESS, tag, MPI_COMM_WORLD, &status);
        }
    }

    size_t const iterations = dimension;
    size_t const a_row = current_row;
    size_t const a_col = current_col;
    size_t const b_row = current_row;
    size_t const b_col = current_col;

    for (size_t l = 0; l < iterations; ++l)
    {
        consistent(a_matrix, b_matrix, c_matrix, current_size);

        if (l + 1 != iterations)
        {
            //A
            {
                size_t const a_next_col = (a_col + 1) % dimension;
                size_t const a_previous_col = (a_col + dimension - 1) % dimension;
                int const send_to = (int)(a_row * dimension + a_next_col);
                int const recv_from = (int)(a_row * dimension + a_previous_col);
                int const current_rank = (int)my_rank;

                if (current_rank < recv_from)
                {
                    MPI_Recv(&additional_a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, recv_from, tag, MPI_COMM_WORLD, &status);
                    MPI_Send(&a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, send_to, tag, MPI_COMM_WORLD);
                    memcpy(&a_matrix[0], &additional_a_matrix[0], current_size * current_size * sizeof(float));
                }
                if (current_rank > recv_from)
                {
                    MPI_Send(&a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, send_to, tag, MPI_COMM_WORLD);
                    MPI_Recv(&a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, recv_from, tag, MPI_COMM_WORLD, &status);
                }
            }
            //B
            {
                size_t const b_next_row = (b_row + 1) % dimension;
                size_t const b_previous_row = (b_row + dimension - 1) % dimension;
                int const send_to = (int)(b_next_row * dimension + current_col);
                int const recv_from = (int)(b_previous_row * dimension + current_col);
                int const current_rank = (int)my_rank;

                if (current_rank < recv_from)
                {
                    MPI_Recv(&additional_a_matrix[0], (int)(current_size * current_size), MPI_FLOAT, recv_from, tag, MPI_COMM_WORLD, &status);
                    MPI_Send(&b_matrix[0], (int)(current_size * current_size), MPI_FLOAT, send_to, tag, MPI_COMM_WORLD);
                    memcpy(&b_matrix[0], &additional_a_matrix[0], current_size * current_size * sizeof(float));
                }
                if (current_rank > recv_from)
                {
                    MPI_Send(&b_matrix[0], (int)(current_size * current_size), MPI_FLOAT, send_to, tag, MPI_COMM_WORLD);
                    MPI_Recv(&b_matrix[0], (int)(current_size * current_size), MPI_FLOAT, recv_from, tag, MPI_COMM_WORLD, &status);
                }
            }
        }
    }

    if (my_rank == 0)
    {
        set_to_result(c_matrix, c, size, current_size, 0, 0);

        for (int i = 1; i < (int)world_size; ++i)
        {
            int const row = i / (int)dimension;
            int const col = i % (int)dimension;

            MPI_Recv(&c_matrix[0], (int)(current_size * current_size), MPI_FLOAT, i, tag, MPI_COMM_WORLD, &status);

            set_to_result(c_matrix, c, size, current_size, row, col);
        }
    }

    if (my_rank != 0)
    {
        MPI_Send(&c_matrix[0], (int)(current_size * current_size), MPI_FLOAT, 0, tag, MPI_COMM_WORLD);
    }

    destroy_matrix(a_matrix);
    destroy_matrix(b_matrix);
    destroy_matrix(c_matrix);
    destroy_matrix(additional_a_matrix);
}

double cannon_method_multiply_time(size_t size, size_t my_rank, size_t world_size)
{
    return calculate_multiply_time(size, my_rank, world_size, cannon_method);
}


int main()
{
    size_t const size = 1000;
    size_t const id = CANNON_METHOD_ALGO_ID;

    char const * name = "Undefined";
    if (id == CONSISTENT_ALGO_ID)
    {
        name = CONSISTENT_ALGO_NAME;
    }
    if (id == TAPE_CIRCUIT_ALGO_ID)
    {
        name = TAPE_CIRCUIT_ALGO_NAME;
    }
    if (id == FOXS_METHOD_ALGO_ID)
    {
        name = FOXS_METHOD_ALGO_NAME;
    }
    if (id == CANNON_METHOD_ALGO_ID)
    {
        name = CANNON_METHOD_ALGO_NAME;
    }

    int my_rank;
    int world_size;

    MPI_Init(NULL, NULL);

    MPI_Comm_size(MPI_COMM_WORLD, &world_size);
    MPI_Comm_rank(MPI_COMM_WORLD, &my_rank);

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
        printf("Algo %s, dimention %zd time %f s\n", name, size, delta);
    }

    MPI_Finalize();
    return 0;
}