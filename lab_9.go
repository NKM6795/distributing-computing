package main

import (
	"fmt"
	"math/rand"
	"time"
)

var size = 1000
var world_size = 4

var done = make(chan bool, 1)

var a = make([]float32, size * size)
var b = make([]float32, size * size)
var c = make([]float32, size * size)

func init_a_b_c() {
	for i := 0; i < size; i++ {
		for j := 0; j < size; j++ {
			a[i * size + j] = (rand.Float32() - 0.5) * 50.0
		}
	}
	for i := 0; i < size; i++ {
		for j := 0; j < size; j++ {
			b[i * size + j] = (rand.Float32() - 0.5) * 50.0
		}
	}
	for i := 0; i < size; i++ {
		for j := 0; j < size; j++ {
			c[i * size + j] = 0.0
		}
	}
}


func consistent_multiply_time() float32 {
	start := time.Now()

	for i := 0; i < size; i++ {
		for j := 0; j < size; j++ {
			temp := c[i * size + j];
			for k := 0; k < size; k++ {
				temp += a[i * size + k] * b[k * size + j];
			}
			c[i * size + j] = temp;
		}
	}

	end := time.Now()
	diff := end.Sub(start)
	return float32(diff) / 1000000000.0
}

func tape_circuit(rank int) {
	current_size := (size / world_size);
    begin := current_size * rank;
	end := begin + current_size;
	
	for i := begin; i < end; i++ {
		for j := 0; j < size; j++ {
			temp := c[i * size + j];
			for k := 0; k < size; k++ {
				temp += a[i * size + k] * b[k * size + j];
			}
			c[i * size + j] = temp;
		}
	}

	done <- true
}

func consistent_tape_circuit_time() float32 {
	start_time := time.Now()

	for i := 0; i < world_size; i++ {
		go tape_circuit(i)
	}
	
	for i := 0; i< world_size; i++ {
		<- done
	}

	end_time := time.Now()
	diff := end_time.Sub(start_time)

	return float32(diff) / 1000000000.0
}

func main() {
	init_a_b_c()

	//fmt.Println(consistent_multiply_time())
	fmt.Println(consistent_tape_circuit_time())
}