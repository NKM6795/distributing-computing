package main

import (
	"fmt"
	"math/rand"
)

var thread_count = 3
var matrix_size = 5
var is_running = true

var matrices = make([][][]int, thread_count)

var matrix_barrier_stop =  make([]chan bool, thread_count)
var matrix_barrier_move =  make([]chan bool, thread_count)
var lock = make(chan bool, 1)
var done = make(chan bool, thread_count)

func barrier() {
	for {
		for i := 0; i < thread_count; i++ {
			<-matrix_barrier_stop[i]
		}
		for i := 0; i < thread_count; i++ {
			matrix_barrier_move[i] <- true
		}
	}
}

func work(index int) {
	for is_running {
		if rand.Intn(2) == 0 {
			x_to_change := rand.Intn(matrix_size)
			y_to_change := rand.Intn(matrix_size)
			
			lock <- true;

			if rand.Intn(2) == 0 && matrices[index][x_to_change][y_to_change] < 10 {
				matrices[index][x_to_change][y_to_change]++
			} else if matrices[index][x_to_change][y_to_change] > 0 {
				matrices[index][x_to_change][y_to_change]--
			} else {
				matrices[index][x_to_change][y_to_change]++
			}
			
			<-lock
		}
		matrix_barrier_stop[index] <- true
		<-matrix_barrier_move[index]

		lock <- true;

		sums := make([]int, thread_count)

		for i := 0; i < thread_count; i++ {
			for j := 0; j < matrix_size; j++ {
				for l := 0; l < matrix_size; l++ {
					sums[i] += matrices[i][j][l]
				}
			}
		}
		
		is_end := true
		for i := 1; i < thread_count; i++ {
			if sums[i - 1] != sums[i] {
				is_end = false;
			}
		}
		if is_end {
			is_running = false
		}

		if !is_running {
			fmt.Println("Complite")
		}

		<-lock

		matrix_barrier_stop[index] <- true
		<-matrix_barrier_move[index]
	}
	done <- true
}

func main() {
	for i := 0; i < thread_count; i++ {
		matrices[i] = make([][]int, matrix_size)
		for j := 0; j < matrix_size; j++ {
			matrices[i][j] = make([]int, matrix_size)
			for l := 0; l < matrix_size; l++ {
				matrices[i][j][l] = rand.Intn(10)
			}
		}
	}

	for i := 0; i < thread_count; i++ {
		matrix_barrier_stop[i] = make(chan bool, 1)
		matrix_barrier_move[i] = make(chan bool, 1)
	}

	go barrier()

	for i := 0; i < thread_count; i++ {
		go work(i)
	}

	for i := 0; i < thread_count; i++ {
		<-done
	}
}